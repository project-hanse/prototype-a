using System;
using System.Collections.Generic;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Dtos
{
	public class OperationTemplate
	{
		public Guid OperationId { get; set; }
		public string OperationName { get; set; }
		public string OperationFullName { get; set; }

		/// <summary>
		/// The dataset types this operation requires.
		/// </summary>
		public IList<DatasetType> InputTypes { get; set; }

		/// <summary>
		/// The dataset type this operation produces.
		/// </summary>
		[Obsolete("This is not used anymore. Use OutputTypes instead.")]
		public DatasetType? OutputType { get; set; }

		/// <summary>
		/// The dataset types this operation produces.
		/// </summary>
		public IList<DatasetType?> OutputTypes { get; set; }

		public string Framework { get; set; }
		public string FrameworkVersion { get; set; }
		public string SectionTitle { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Signature { get; set; }
		public string SignatureName { get; set; }
		public string ReturnsDescription { get; set; }
		public string SourceUrl { get; set; }
		public Dictionary<string, string> DefaultConfig { get; set; }

		[Obsolete("Replaced by OutputType")] public string Returns { get; set; }
	}
}
