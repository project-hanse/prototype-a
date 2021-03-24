export interface FrontendExecutionNotification {
  PipelineId: string;
  ExecutionId: string;
  BlockId: string;
  Successful: boolean;
  ExecutionTime: any;
  ErrorDescription: string;
  NodesExecuted: number;
  NodesInExecution: number;
  ToBeExecuted: number;
}
