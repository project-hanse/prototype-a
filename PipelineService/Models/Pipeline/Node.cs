using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PipelineService.Models.Pipeline
{
	/// <summary>
	/// A <c>Node</c> is part of a pipeline.
	/// A <c>Node</c> describes an operation performed on one or more input datasets and produces one dataset.
	/// A <c>Node</c> can have no, one or multiple successor Blocks.
	/// </summary>
	[Table(nameof(Node))]
	public abstract record Node : BasePersistentModel
	{
		/// <summary>
		/// The pipeline's id this node belongs to.
		/// </summary>
		public Guid PipelineId { get; set; }

		/// <summary>
		/// The nodes successors.
		/// </summary>
		[Column("HAS_SUCCESSOR")]
		public IList<Node> Successors { get; set; } = new List<Node>();

		/// <summary>
		/// The operation that will be performed on the input dataset.
		/// </summary>
		public string Operation { get; set; }

		/// <summary>
		/// A textual description displayed to the user (developer).
		/// </summary>
		public string OperationDescription { get; set; }

		/// <summary>
		/// The id of the operation that will be performed on the input dataset.
		/// </summary>
		public Guid OperationId { get; set; }

		/// <summary>
		/// The configuration of the operation (usually corresponds to function parameters).
		/// </summary>
		[JsonIgnore]
		public IDictionary<string, string> OperationConfiguration
		{
			get => JsonConvert.DeserializeObject<Dictionary<string, string>>(OperationConfigurationSerialized);
			set => OperationConfigurationSerialized = JsonConvert.SerializeObject(value);
		}

		public string OperationConfigurationSerialized { get; set; } = "{}";

		[JsonIgnore] public abstract string ResultKey { get; }

		/// <summary>
		/// Any value returned here will be included in the hash value of this object.
		/// </summary>
		public abstract string IncludeInHash { get; }

		protected static string IdOrHash(Guid? datasetId, string datasetHash)
		{
			return datasetId.HasValue ? datasetId.Value.ToString() : datasetHash;
		}
	}
}
