using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IOperationTemplatesService
	{
		Task<IList<OperationTemplate>> GetOperationDtos();
	}
}
