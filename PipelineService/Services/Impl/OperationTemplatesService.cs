using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Extensions;
using PipelineService.Models.Constants;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;

namespace PipelineService.Services.Impl
{
	public class OperationTemplatesService : IOperationTemplatesService
	{
		private const string OperationTemplatesCacheKey = "OperationTemplates-29d583f5-9caa-49a6-8345-9536576fb969";

		private readonly ILogger<OperationTemplatesService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMemoryCache _memoryCache;

		private string OperationTemplatesPath => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("OperationTemplatesFolder", ""));

		public OperationTemplatesService(
			ILogger<OperationTemplatesService> logger,
			IConfiguration configuration,
			IMemoryCache memoryCache)
		{
			_logger = logger;
			_configuration = configuration;
			_memoryCache = memoryCache;
		}

		public async Task<IList<OperationTemplate>> GetOperationDtos()
		{
			if (_memoryCache.TryGetValue(OperationTemplatesCacheKey, out IList<OperationTemplate> operations))
			{
				_logger.LogDebug("OperationTemplates found in cache");
				return operations;
			}

			_logger.LogDebug("Loading available operations");
			operations = await LoadTemplatesFromFiles();
			operations = operations
				.OrderBy(op => op.Framework)
				.ThenBy(op => op.SectionTitle)
				.ThenBy(op => op.OperationName)
				.ToList();

			_logger.LogInformation("Loaded {OperationsCount} operations", operations.Count);

			_memoryCache.Set(OperationTemplatesCacheKey, operations, TimeSpan.FromHours(6));

			return operations;
		}

		public async Task<OperationTemplate> GetTemplate(Guid operationId, string operationIdentifier)
		{
			_logger.LogDebug("Loading operation template for {OperationId} {OperationIdentifier}",
				operationId, operationIdentifier);

			// TODO make this more efficient
			var template = (await GetOperationDtos())
				.FirstOrDefault(op => op.OperationId == operationId && op.OperationName == operationIdentifier);

			if (template == default)
			{
				_logger.LogInformation("Operation template not found for {OperationId} {OperationIdentifier}",
					operationId, operationIdentifier);

				return null;
			}

			_logger.LogInformation("Loaded operation template for {OperationId} {OperationIdentifier}",
				operationId, operationIdentifier);
			return template;
		}

		private async Task<IList<OperationTemplate>> LoadTemplatesFromFiles()
		{
			var operations = new List<OperationTemplate>();
			var files = Directory.GetFiles(OperationTemplatesPath, "*.json");
			foreach (var file in files)
			{
				var operation = await LoadOperationTemplatesFromFile(file);
				operations.AddRange(operation);
			}

			if (operations.Count == 0)
			{
				_logger.LogWarning("No operations found in {OperationTemplatesPath}", OperationTemplatesPath);
			}

			return operations;
		}

		private async Task<IEnumerable<OperationTemplate>> LoadOperationTemplatesFromFile(string file)
		{
			if (!File.Exists(file))
			{
				_logger.LogWarning("File {File} does not exist", file);
				return new List<OperationTemplate>();
			}

			using var streamReader = new StreamReader(file);

			var content = await streamReader.ReadToEndAsync();

			return JsonConvert.DeserializeObject<IList<OperationTemplate>>(content);
		}

		[Obsolete("Deprecated in favor of LoadTemplatesFromFiles() - Postprocessing is no longer needed")]
		private async Task<IList<OperationTemplate>> LoadFromConfigFiles()
		{
			_logger.LogDebug("Loading operation templates from config files");
			var operations = new List<OperationTemplate>();
			if (OperationTemplatesPath == null || !Directory.Exists(OperationTemplatesPath))
			{
				_logger.LogWarning("No operation template configuration files found");
				return new List<OperationTemplate>();
			}

			foreach (var configFile in Directory.GetFiles(OperationTemplatesPath))
			{
				_logger.LogDebug("Loading operations from {OperationsConfigFile}", configFile);
				using var streamReader = new StreamReader(configFile);
				var content = await streamReader.ReadToEndAsync();
				var operationDtos = JsonConvert.DeserializeObject<IList<OperationTemplate>>(content);
				operations.AddRange(operationDtos);
				_logger.LogInformation("Loaded {OperationsCount} operation templates from {OperationsConfigFile}",
					operationDtos.Count, configFile);
			}

			_logger.LogDebug("Postprocessing configured operations");

			foreach (var operationDto in operations.Where(operationDto => operationDto.Framework == "pandas"))
			{
				operationDto.OperationName = operationDto.SignatureName;
				operationDto.OperationFullName = operationDto.Title;
				operationDto.OperationId = OperationIds.OpIdPdSingleGeneric;
				operationDto.InputTypes =
					operationDto.Signature.Contains("other") ||
					operationDto.Signature.Contains("right") // TODO: this should check the parameters type
						? new List<DatasetType> { DatasetType.PdDataFrame, DatasetType.PdDataFrame }
						: new List<DatasetType> { DatasetType.PdDataFrame };
				if (operationDto.Returns.Contains("DataFrame"))
				{
					operationDto.OutputType = DatasetType.PdDataFrame;
				}
				else if (operationDto.Returns.Contains("Series"))
				{
					operationDto.OutputType = DatasetType.PdSeries;
				}
				else
				{
					operationDto.OutputType = null;
				}

				operationDto.OutputTypes = new List<DatasetType?> { operationDto.OutputType };

				if (operationDto.DefaultConfig == null)
				{
					operationDto.DefaultConfig = new Dictionary<string, string>();
					var arguments = operationDto.Signature
						.Substring(operationDto.Signature.IndexOf('(') + 1)
						.ReplaceLastOccurenceOf(")", "")
						.Split(", ");

					foreach (var argument in arguments)
					{
						var split = argument.Split("=");
						if (split.Length == 1)
						{
							operationDto.DefaultConfig.Add(argument, "");
						}

						if (split.Length == 2)
						{
							operationDto.DefaultConfig.Add(split[0], split[1].Replace("'", ""));
						}
					}
				}
			}

			return operations;
		}

		[Obsolete("Deprecated in favor of LoadTemplatesFromFiles() - Operation templates are specified in JSON format")]
		private static IEnumerable<OperationTemplate> GenerateOperationsFromIds()
		{
			var fromIds = new List<OperationTemplate>();
			var type = typeof(OperationIds);
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (var fieldInfo in fields)
			{
				var operation = new OperationTemplate
				{
					OperationFullName = fieldInfo.Name,
					OperationId = Guid.Parse(fieldInfo.GetValue(null)?.ToString() ?? string.Empty)
				};

				var split = Regex.Split(operation.OperationFullName.Replace("OpId", ""), @"(?<!^)(?=[A-Z])");
				operation.Framework = split[0] switch
				{
					"Pd" => "pandas",
					"Sklearn" => "scikit-learn",
					_ => "unknown"
				};

				operation.InputTypes = split[1] switch
				{
					"File" => new List<DatasetType> { DatasetType.File },
					"Single" => new List<DatasetType> { DatasetType.PdDataFrame },
					"Double" => new List<DatasetType> { DatasetType.PdDataFrame, DatasetType.PdDataFrame },
					_ => new List<DatasetType> { }
				};

				operation.OperationName = string.Join("_", split, 2, split.Length - 2).ToLower();

				operation.DefaultConfig = new Dictionary<string, string>();
				operation.Returns = "DataFrame";
				operation.Title = operation.OperationFullName;
				operation.SectionTitle = "Custom Operations";

				fromIds.Add(operation);
			}

			return fromIds;
		}
	}
}
