using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Models.Constants;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;

namespace PipelineService.Services.Impl
{
    public class OperationsService : IOperationsService
    {
        private readonly ILogger<OperationsService> _logger;
        private readonly IConfiguration _configuration;

        private string OperationsConfigPath => Path.Combine(
            _configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
            _configuration.GetValue("OperationsConfigFolder", ""));

        public OperationsService(
            ILogger<OperationsService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IList<OperationDto>> GetOperationDtos()
        {
            _logger.LogDebug("Loading available operations");
            var operations = new List<OperationDto>();
            operations.AddRange(GenerateOperationsFromIds());
            operations.AddRange(await LoadFromConfigFiles());
            operations = operations
                .OrderBy(op => op.Framework)
                .ThenBy(op => op.SectionTitle)
                .ThenBy(op => op.OperationName)
                .ToList();

            _logger.LogInformation("Loaded {OperationsCount} operations", operations.Count);

            return operations;
        }

        private async Task<IList<OperationDto>> LoadFromConfigFiles()
        {
            _logger.LogDebug("Loading operations from config files");
            var operations = new List<OperationDto>();
            if (OperationsConfigPath == null || !Directory.Exists(OperationsConfigPath))
            {
                _logger.LogWarning("No operation configuration files found");
                return new List<OperationDto>();
            }

            foreach (var configFile in Directory.GetFiles(OperationsConfigPath))
            {
                _logger.LogDebug("Loading operations from {OperationsConfigFile}", configFile);
                using var streamReader = new StreamReader(configFile);
                var content = await streamReader.ReadToEndAsync();
                var operationDtos = JsonConvert.DeserializeObject<IList<OperationDto>>(content);
                operations.AddRange(operationDtos);
                _logger.LogInformation("Loaded {OperationsCount} operations from {OperationsConfigFile}",
                    operationDtos.Count, configFile);
            }

            _logger.LogDebug("Postprocessing configured operations");

            foreach (var operationDto in operations.Where(operationDto => operationDto.Framework == "pandas"))
            {
                operationDto.OperationName = operationDto.SignatureName;
                operationDto.OperationFullName = operationDto.Title;
                operationDto.OperationId = OperationIds.OpIdPdSingleGeneric;
                operationDto.OperationInputType =
                    operationDto.Signature.Contains("other") // TODO: this should check the parameters type
                        ? OperationInputTypes.Double
                        : OperationInputTypes.Single;
                if (operationDto.DefaultConfig == null)
                {
                    operationDto.DefaultConfig = new Dictionary<string, string>();
                    var arguments = operationDto.Signature
                        .Substring(operationDto.Signature.IndexOf('(') + 1)
                        .Replace(")", "")
                        .Split(", ");

                    foreach (var argument in arguments)
                    {
                        var split = argument.Split("=");
                        if (split.Length == 1)
                        {
                            operationDto.DefaultConfig.Add(split[0], "NO_DEFAULT_FOUND");
                        }

                        if (split.Length == 2)
                        {
                            operationDto.DefaultConfig.Add(split[0], split[1]);
                        }
                    }
                }
            }

            return operations;
        }

        private static IEnumerable<OperationDto> GenerateOperationsFromIds()
        {
            var fromIds = new List<OperationDto>();
            var type = typeof(OperationIds);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                var operation = new OperationDto
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

                operation.OperationInputType = split[1] switch
                {
                    "File" => OperationInputTypes.File,
                    "Single" => OperationInputTypes.Single,
                    "Double" => OperationInputTypes.Double,
                    _ => OperationInputTypes.Unknown
                };

                operation.OperationName = string.Join("_", split, 2, split.Length - 2).ToLower();

                fromIds.Add(operation);
            }

            return fromIds;
        }
    }
}