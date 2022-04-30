using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos
{
	public class OperationTuples
	{
		public string TupleDescription { get; set; }
		public string PredecessorOperationIdentifier { get; set; }
		public IDictionary<string, string> PredecessorOperationConfiguration { get; set; }
		public IList<Dataset> PredecessorOperationInputs { get; set; }
		public IList<Dataset> PredecessorOperationOutput { get; set; }
		public string TargetOperationIdentifier { get; set; }
		public IList<Dataset> TargetInputs { get; set; }
	}
}
