using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos;

public class PipelineCandidate
{
	[JsonProperty("pipeline_id")]
	public Guid PipelineId { get; set; }

	[JsonProperty("started_at")]
	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime StartedAt { get; set; }

	[JsonProperty("completed_at")]
	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime CompletedAt { get; set; }

	[JsonProperty("batch_number")]
	public int BatchNumber { get; set; }

	/// <summary>
	/// The OpenML task id that this pipeline candidate is trying to solve.
	/// </summary>
	[JsonProperty("task_id")]
	public long TaskId { get; set; }

	/// <summary>
	/// The OpenML dataset id that is the initial input for this pipeline candidate.
	/// </summary>
	[JsonProperty("dataset_id")]
	public long DatasetId { get; set; }

	/// <summary>
	/// The type of task that this pipeline candidate is trying to solve (e.g. SUPERVISED_CLASSIFICATION).
	/// </summary>
	[JsonProperty("task_type_id")]
	public string TaskTypeId { get; set; }

	/// <summary>
	/// A list of actions to take to produce a pipeline.
	/// </summary>
	[JsonProperty("actions")]
	public IList<PipelineAction> Actions { get; set; }

	[JsonProperty("created_by")]
	public string CreatedBy { get; set; }

	[JsonProperty("abort")]
	public bool? Aborted { get; set; }

	[JsonProperty("reward_function_type")]
	public string RewardFunctionType { get; set; }

	[JsonProperty("sleep_time_after_new_actions")]
	public float SleepTimeAfterNewActions { get; set; } = 1.0f;

	[JsonProperty("max_actions_per_pipeline")]
	public int MaxActionsPerPipeline { get; set; } = 25;

	[JsonProperty("mcts_iteration_limit")]
	public int MctsIterationLimit { get; set; }	= 15;

	[JsonProperty("target_action_count")]
	public int TargetActionCount { get; set; } = 13;

	[JsonProperty("expert_policy_model_name")]
	public string ExpertPolicyModelName { get; set; } = "composite";

	[JsonProperty("expert_policy_probability")]
	public float ExpertPolicyProbability { get; set; } = 0.75f;

	public string SourceFileName { get; set; }

	public double SimulationDuration => Math.Max((CompletedAt - StartedAt).TotalMilliseconds, 0);

	private int? _actionsCount;

	public int ActionsCount
	{
		get => _actionsCount ?? (_actionsCount = Actions?.Count ?? 0).Value;
		set => _actionsCount = value;
	}
}

/// <summary>
/// A <c>PipelineAction</c> is a single action to take to append a new operation to a pipeline.
/// This is produced by a MCTS algorithm.
/// </summary>
public class PipelineAction
{
	[JsonProperty("operation")]
	public OperationTemplate Operation { get; set; }

	[JsonProperty("input_datasets")]
	public IList<Dataset> InputDatasets { get; set; }

	[JsonProperty("output_datasets")]
	public IList<Dataset> OutputDatasets { get; set; }
}
