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
		/// </summary>
		public Guid OperationId { get; set; }

		/// <summary>
		/// The operation that will be performed on the input dataset.
		/// </summary>
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
		/// The output of this operation.
		/// </summary>
		[JsonIgnore]
		public Dataset Output { get; set; } = new();

		public string OutputSerialized
		{
			get => Output != null ? JsonConvert.SerializeObject(Output) : null;
			set => Output = JsonConvert.DeserializeObject<Dataset>(value);
		}

		/// <summary>
		/// A hash of all values required to check if the operation has changed.
		/// </summary>
		public string ComputedHash
		{
			get
			{
				var include = new List<string>
				{
					PipelineId.ToString(),
					OperationId.ToString(),
					InputsSerialized,
					OperationIdentifier,
					OperationConfigurationSerialized
				};
				return HashHelper.ComputeHash(include);
			}
		}
	}
}
