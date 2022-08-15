namespace PipelineService.Models.Enums;

public enum ExecutionStrategy
{
	/// <summary>
	/// Schedules the execution of each operation as late as possible in relation to the dependent operations.
	/// </summary>
	Lazy,

	/// <summary>
	/// Schedules the execution of each operation as soon as possible in relation to the dependent operations.
	/// </summary>
	Eager
}
