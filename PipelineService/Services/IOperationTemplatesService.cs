using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IOperationTemplatesService
	{
		Task<IList<OperationTemplate>> GetOperationDtos(GetOperationTemplatesRequest request);
		Task<OperationTemplate> GetTemplate(Guid operationId, string operationName);
	}
}
