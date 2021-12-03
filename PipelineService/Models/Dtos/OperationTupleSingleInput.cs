using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos
{
	public class OperationTupleSingleInput
	{
		public string Description { get; set; }
		public string DatasetHash { get; set; }
		public Guid NodeId { get; set; }
		public Guid OperationId { get; set; }
		public string OperationIdentifier { get; set; }
		public IDictionary<string, string> OperationConfiguration { get; set; }
		public Guid TargetNodeId { get; set; }
		public Guid TargetOperationId { get; set; }
		public string TargetOperation { get; set; }
		public IList<Dataset> OperationInputs { get; set; }
		public Dataset OperationOutput { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is OperationTupleSingleInput typed)
			{
				return Equals(typed);
			}

			return false;
		}

		private bool Equals(OperationTupleSingleInput other)
		{
			return DatasetHash == other.DatasetHash && NodeId.Equals(other.NodeId) &&
			       TargetNodeId.Equals(other.TargetNodeId) && Description == other.Description;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(DatasetHash, NodeId, TargetNodeId, Description);
		}
	}
}
