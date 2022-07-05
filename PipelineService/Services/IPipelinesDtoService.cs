using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IPipelinesDtoService
	{
		public Task<IList<OperationTuples>> GetOperationTuples();
		public Task<PipelineExport> ExportPipeline(Guid pipelineId);

		/// <summary>
		/// Parses the data in the <c>exportObject</c> and creates a new pipeline.
		/// </summary>
		/// <param name="exportObject"></param>
		/// <exception cref="InvalidDataException">If the provided data in the export object can not be parsed.</exception>
		/// <returns></returns>
		public Task<Guid> ImportPipeline(PipelineExport exportObject);

		public Task<Guid> ImportPipelineCandidate(PipelineCandidate pipelineCandidate, string username = null);

		/// <summary>
		/// Loads a number of pipeline candidates, tries to import them, and checks if they are executable.
		/// </summary>
		/// <para name="numberOfCandidates">The number of candidates that will be processed.</para>
		Task<int> ProcessPipelineCandidates(int numberOfCandidates);
	}
}
