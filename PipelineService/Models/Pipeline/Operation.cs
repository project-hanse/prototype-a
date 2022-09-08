using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PipelineService.Helper;

namespace PipelineService.Models.Pipeline
{
	/// <summary>
	/// An <c>Operation</c> is part of a pipeline.
	/// An <c>Operation</c> describes an operation performed on one or more input datasets and produces one dataset.
	/// Am <c>Operation</c> can have zero, one or multiple successor <c>Operation</c>s.
	/// </summary>
	[Table(nameof(Operation))]
	public record Operation : BasePersistentModel
	{
		/// <summary>
		/// The pipeline's id this node belongs to.
		/// </summary>
		public Guid PipelineId { get; set; }

		/// <summary>
		/// The nodes successors.
		/// </summary>
		[Column("HAS_SUCCESSOR")]
		public IList<Operation> Successors { get; set; } = new List<Operation>();

		/// <summary>
		/// The id of the operation that will be performed on the input dataset.
		/// Used by workers to perform the appropriate operation.
		/// </summary>
		public Guid OperationId { get; set; }

		/// <summary>
		/// The operation that will be performed on the input dataset.
		/// A string used by workers to perform a specific variant of an operation.
		/// </summary>
		/// <remarks>
		///	Example: For the generic pandas operation this could be "dropna", "fillna", etc..
		/// </remarks>
		public string OperationIdentifier { get; set; }

		/// <summary>
		/// A textual description displayed to the user (developer).
		/// </summary>
		public string OperationDescription { get; set; }

		/// <summary>
		/// The configuration of the operation (usually corresponds to function parameters).
		/// </summary>
		[JsonIgnore]
		public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();

		public string OperationConfigurationSerialized
		{
			get => JsonConvert.SerializeObject(OperationConfiguration);
			set => OperationConfiguration = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
		}

		/// <summary>
		/// The input datasets of this operation.
		/// </summary>
		[JsonIgnore]
		public IList<Dataset> Inputs { get; private set; } = new List<Dataset>();

		public string InputsSerialized
		{
			get => JsonConvert.SerializeObject(Inputs);
			set => Inputs = JsonConvert.DeserializeObject<IList<Dataset>>(value);
		}

		/// <summary>
		/// The outputs of this operation.
		/// </summary>
		[JsonIgnore]
		public IList<Dataset> Outputs { get; set; } = new List<Dataset>();

		/// <summary>
		/// Should be <c>OutputsSerialized</c> but kept OutputSerialized for backwards compatibility.
		/// </summary>
		public string OutputSerialized
		{
			get => Outputs != null ? JsonConvert.SerializeObject(Outputs) : null;
			set
			{
				if (value.Trim().StartsWith("{"))
					value = $"[{value}]";
				Outputs = JsonConvert.DeserializeObject<IList<Dataset>>(value);
			}
		}

		/// <summary>
		/// A hash of all values required to check if the operation has changed.
		/// </summary>
		public string OperationHash
		{
			get
			{
				var include = new List<string>
				{
					PipelineId.ToString(),
					OperationId.ToString(),
					InputsSerialized,
					OperationIdentifier,
					OperationConfigurationSerialized,
					PredecessorsHash
				};
				return HashHelper.ComputeHash(include);
			}
		}

		/// <summary>
		/// A hash of all predecessor's hashes.
		/// </summary>
		public string PredecessorsHash { get; set; }
	}
}
