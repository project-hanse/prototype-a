using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.Constants;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;

namespace PipelineService.Services.Impl
{
    public class OperationsService : IOperationsService
    {
        private readonly ILogger<OperationsService> _logger;

        public OperationsService(ILogger<OperationsService> logger)
        {
            _logger = logger;
        }

        public Task<IList<OperationDto>> GetOperationDtos()
        {
            _logger.LogDebug("Loading available operations");
            var operations = new List<OperationDto>();
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

                operations.Add(operation);
            }

            operations = operations
                .OrderBy(op => op.Framework)
                .ThenBy(op => op.OperationName)
                .ToList();

            _logger.LogInformation("Loaded {OperationsCount} operations", operations.Count);

            return Task.FromResult<IList<OperationDto>>(operations);
        }
    }
}