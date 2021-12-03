import {Dataset} from './dataset';

export interface FrontendExecutionNotification {
	PipelineId: string;
	ExecutionId: string;
	OperationId: string;
	OperationName: string;
	Successful: boolean;
	ExecutionTime: any;
	CompletedAt: Date;
	ErrorDescription: string;
	OperationsExecuted: number;
	OperationsInExecution: number;
	OperationsToBeExecuted: number;
	OperationsFailedToExecute: number;
	ResultDatasetKey: string;
	ResultDataset: Dataset;
}
