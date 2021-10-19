export interface FrontendExecutionNotification {
	PipelineId: string;
	ExecutionId: string;
	NodeId: string;
	OperationName: string;
	Successful: boolean;
	ExecutionTime: any;
	CompletedAt: Date;
	ErrorDescription: string;
	NodesExecuted: number;
	NodesInExecution: number;
	NodesToBeExecuted: number;
	NodesFailedToExecute: number;
	ResultDatasetKey: string;
}
