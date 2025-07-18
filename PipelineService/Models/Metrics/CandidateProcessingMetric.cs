using System;
using System.Collections.Generic;
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
	public string ErrorMessage { get; set; }

	/// <summary>
	/// The time the candidate was created on.
	/// </summary>
	public DateTime? CandidateCreatedOn { get; set; }

	/// <summary>
	/// The time the candidate simulation was started.
	/// </summary>
	public DateTime? SimulationStartTime { get; set; }

	/// <summary>
	/// The time the candidate simulation was ended.
	/// </summary>
	public DateTime? SimulationEndTime { get; set; }

	/// <summary>
	/// The time the processing of the candidate started.
	/// </summary>
	public DateTime? ProcessingStartTime { get; set; }

	/// <summary>
	/// The time the import of the candidate started.
	/// </summary>
	public DateTime? ImportStartTime { get; set; }

	/// <summary>
	/// The time the import of the candidate finished.
	/// </summary>
	public DateTime? ImportEndTime { get; set; }

	/// <summary>
	/// The time the processing of the candidate ended.
	/// </summary>
	public DateTime? ProcessingEndTime { get; set; }

	/// <summary>
	/// The number of actions in a pipeline candidate that are processed.
	/// </summary>
	public int ActionCount { get; set; }

	public int OperationCount { get; set; }

	/// <summary>
	/// The OpenML task ID of the pipeline candidate.
	/// </summary>
	[MaxLength(255)]
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
	/// Stores the processing duration (in ms) persistently to the database one a candidate has been processed.
	/// This can then be used to compute average processing times in the database.
	/// </summary>
	public double? ProcessingDurationP { get; set; }

	/// <summary>
	/// The time it took to import the pipeline candidate.
	/// </summary>
	[NotMapped]
	public double ProcessingDuration => ProcessingEndTime.HasValue && ProcessingStartTime.HasValue
		? Math.Max((ProcessingEndTime.Value - ProcessingStartTime.Value).TotalMilliseconds, 0)
		: 0;

	/// <summary>
	/// The time it took to process the pipeline candidate.
	/// </summary>
	[NotMapped]
	public double ImportDuration => ImportStartTime.HasValue && ImportEndTime.HasValue
		? Math.Max((ImportEndTime.Value - ImportStartTime.Value).TotalMilliseconds, 0)
		: 0;

	/// <summary>
	/// The time it took to simulate the pipeline candidate.
	/// </summary>
	[NotMapped]
	public double SimulationDuration => SimulationStartTime.HasValue && SimulationEndTime.HasValue
		? Math.Max((SimulationEndTime.Value - SimulationStartTime.Value).TotalMilliseconds, 0)
		: 0;

	/// <summary>
	/// Indicated whether the pipeline candidate was imported successfully.
	/// </summary>
	public bool ImportSuccess { get; set; }

	/// <summary>
	/// True if the simulation was aborted after a specified maximum number of actions.
	/// </summary>
	public bool Aborted { get; set; }

	/// <summary>
	/// The reward function used to simulate this pipeline candidate.
	/// </summary>
	[MaxLength(255)]
	public string RewardFunctionType { get; set; }

	/// <summary>
	/// The number of attempts this pipeline candidate was tried to be executed. Includes the first attempt and all
	/// attempts with randomized configurations.
	/// </summary>
	public int ExecutionAttempts { get; set; } = 1;

	/// <summary>
	/// The number of operations that were randomized per attempt.
	/// </summary>
	public IDictionary<int, int> OperationsRandomizedCount { get; set; } = new Dictionary<int, int>();

	/// <summary>
	/// Indicates if the candidate processing was completed.
	/// </summary>
	/// <remarks>
	/// Enables completion of candidate processing after application reboot.
	/// </remarks>
	public bool ProcessingCompleted { get; set; } = false;

	public float SleepTimeAfterNewActions { get; set; } = 1.0f;
	public int MaxActionsPerPipeline { get; set; } = 25;
	public int MctsIterationLimit { get; set; } = 15;
	public int TargetActionCount { get; set; } = 13;
	public string ExpertPolicyModelName { get; set; } = "composite";
	public float ExpertPolicyProbability { get; set; } = 0.75f;
}
