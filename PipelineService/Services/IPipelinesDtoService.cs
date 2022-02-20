using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IPipelinesDtoService
	{
		public Task<IList<OperationTuples>> GetOperationTuples();
		public Task<PipelineExport> ExportPipeline(Guid pipelineId);
	}
}
