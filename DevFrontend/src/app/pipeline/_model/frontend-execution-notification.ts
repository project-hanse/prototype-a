import {Dataset} from './dataset';

export interface FrontendExecutionNotification {
	PipelineId: string;
	ExecutionId: string;
	OperationId: string;
	OperationName: string;
	Successful: boolean;
	Cached: boolean;
	ExecutionTime: any;
	CompletedAt: Date;
	ErrorDescription: string;
	OperationsExecuted: number;
	OperationsInExecution: number;
	OperationsToBeExecuted: number;
	OperationsFailedToExecute: number;
	ResultDatasets: Array<Dataset>;
}
