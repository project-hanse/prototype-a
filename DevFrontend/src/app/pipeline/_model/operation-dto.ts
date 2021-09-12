export interface OperationDto {
  operationId: string;
  operationName: string;
  operationFullName: string;
  framework: string;
  operationInputType: OperationInputTypes;
}

export enum OperationInputTypes {
  File = 0,
  Single = 1,
  Double = 2,
  Unknown
}
