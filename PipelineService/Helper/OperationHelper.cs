using System;
using PipelineService.Models.Dtos;

namespace PipelineService.Helper;

public static class OperationHelper
{
	public static string GetGlobalUniqueOperationIdentifier(Guid operationId, string operationIdentifier)
	{
		return $"{operationId}-{operationIdentifier}";
	}

	public static string GetGlobalUniqueOperationIdentifier(OperationTemplate operationTemplate)
	{
		return GetGlobalUniqueOperationIdentifier(operationTemplate.OperationId, operationTemplate.OperationName);
	}
}
