using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IPipelinesDtoService
	{
		public Task<IList<OperationTupleSingleInput>> GetSingleInputNodeTuples();
		public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples();
		public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples(Guid pipelineId);
	}
}
