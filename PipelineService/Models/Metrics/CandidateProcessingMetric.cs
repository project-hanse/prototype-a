using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineService.Models.Metrics;

/// <summary>
/// Collects metrics when importing a pipeline candidate.
/// </summary>
public record CandidateProcessingMetric : BasePersistentModel
{
	/// <summary>
	/// Whether the candidate was imported successfully.
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// A string representation of the error if the candidate was not imported successfully.
	/// </summary>
	public string Error { get; set; }

	/// <summary>
	/// The time the candidate was created on.
	/// </summary>
	public DateTime CandidateCreatedOn { get; set; }

	/// <summary>
	/// The time the processing of the candidate started.
	/// </summary>
	public DateTime ProcessingStartTime { get; set; }

	/// <summary>
	/// The time the import of the candidate started.
	/// </summary>
	public DateTime ImportStartTime { get; set; }

	/// <summary>
	/// The time the import of the candidate finished.
	/// </summary>
	public DateTime ImportEndTime { get; set; }

	/// <summary>
	/// The time the processing of the candidate ended.
	/// </summary>
	public DateTime ProcessingEndTime { get; set; }

	/// <summary>
	/// The number of actions in a pipeline candidate that are processed.
	/// </summary>
	public int ActionCount { get; set; }

	/// <summary>
	/// The OpenML task ID of the pipeline candidate.
	/// </summary>
	[MaxLength(256)]
	public long TaskId { get; set; }

	/// <summary>
	/// The pipeline candidate's batch number.
	/// </summary>
	public int BatchNumber { get; set; }

	/// <summary>
	/// The pipeline candidate's ID.
	/// </summary>
	public Guid PipelineId { get; set; }

	/// <summary>
	/// The time it took to import the pipeline candidate.
	/// </summary>
	[NotMapped]
	public double ProcessingDuration => (ProcessingEndTime - ProcessingStartTime).TotalMilliseconds;

	/// <summary>
	/// The time it took to process the pipeline candidate.
	/// </summary>
	[NotMapped]
	public double ImportDuration => (ImportEndTime - ImportStartTime).TotalMilliseconds;

	/// <summary>
	/// Indicated whether the pipeline candidate was imported successfully.
	/// </summary>
	public bool ImportSuccess { get; set; }
}
