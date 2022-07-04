using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineService.Models.Metrics;

/// <summary>
/// Collects metrics when importing a pipeline candidate.
/// </summary>
public record CandidateImportMetric : BasePersistentModel
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
	/// The time the import started.
	/// </summary>
	public DateTime StartTime { get; set; }

	/// <summary>
	/// The time the import finished.
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	/// The number of actions in a pipeline candidate that are processed.
	/// </summary>
	public int ActionCount { get; set; }

	/// <summary>
	/// The OpenML task ID of the pipeline candidate.
	/// </summary>
	[MaxLength(256)]
	public string TaskId { get; set; }

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
	public TimeSpan ImportDuration => EndTime - StartTime;
}
