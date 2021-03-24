export interface FrontendExecutionNotification {
  PipelineId: string;
  ExecutionId: string;
  BlockId: string;
  OperationName: string;
  Successful: boolean;
  ExecutionTime: any;
  CompletedAt: Date;
  ErrorDescription: string;
  NodesExecuted: number;
  NodesInExecution: number;
  ToBeExecuted: number;
  ResultDatasetKey: string;
}
