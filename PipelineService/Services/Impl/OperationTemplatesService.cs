using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Helper;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;

namespace PipelineService.Services.Impl
{
	public class OperationTemplatesService : IOperationTemplatesService
	{
		private const string OperationTemplatesCacheKey = "OperationTemplates-29d583f5-9caa-49a6-8345-9536576fb969";

		private readonly ILogger<OperationTemplatesService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IMemoryCache _memoryCache;

		private string OperationTemplatesPath => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("OperationTemplatesFolder", ""));

		public OperationTemplatesService(
			ILogger<OperationTemplatesService> logger,
			IConfiguration configuration,
			IPipelinesDao pipelinesDao,
			IMemoryCache memoryCache)
		{
			_logger = logger;
			_configuration = configuration;
			_pipelinesDao = pipelinesDao;
			_memoryCache = memoryCache;
		}

		public async Task<IList<OperationTemplate>> GetOperationDtos(GetOperationTemplatesRequest request)
		{
			if (_memoryCache.TryGetValue(request, out IList<OperationTemplate> operations))
			{
				_logger.LogDebug("No OperationTemplates in cache for request {@GetOperationTemplatesRequest}", request);
				return operations;
			}

			_logger.LogDebug("Loading available operations");
			operations = await LoadTemplatesFromFiles();
			operations = operations
				.OrderBy(op => op.Framework)
				.ThenBy(op => op.SectionTitle)
				.ThenBy(op => op.OperationName)
				.ToList();

			if (request.FilterUnused)
			{
				_logger.LogDebug("Removing operation templates that have not been used in a pipeline");
				var usedOperationIds = (await _pipelinesDao.GetUsedOperationIdentifiers(true)).ToList();
				operations = operations
					.Where(op => usedOperationIds.Contains(OperationHelper.GetGlobalUniqueOperationIdentifier(op)))
					.ToList();
			}

			_logger.LogInformation("Loaded {OperationsCount} operations", operations.Count);

			_memoryCache.Set(request, operations,
				TimeSpan.FromMinutes(_configuration.GetValue("Cache:OperationTemplatesMinutes", 15)));

			return operations;
		}

		public async Task<OperationTemplate> GetTemplate(Guid operationId, string operationName)
		{
			_logger.LogDebug("Loading operation template for {OperationId} {OperationName}", operationId, operationName);

			// TODO make this more efficient
			var template = (await GetOperationDtos(new GetOperationTemplatesRequest()))
				.FirstOrDefault(op => op.OperationId == operationId && op.OperationName == operationName);

			if (template == default)
			{
				_logger.LogInformation("Operation template not found for {OperationId} {OperationName}",
					operationId, operationName);

				return null;
			}

			_logger.LogInformation("Loaded operation template for {OperationId} {OperationName}", operationId, operationName);
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

			var ops = JsonConvert.DeserializeObject<IList<OperationTemplate>>(content);

			if (ops == null) return new List<OperationTemplate>();

			foreach (var operationTemplate in ops)
			{
				if (operationTemplate.OutputTypes == null || operationTemplate.OutputTypes?.Count == 0)
				{
					operationTemplate.OutputTypes = new List<DatasetType?> { operationTemplate.OutputType };
				}
			}

			return ops;
		}
	}
}
