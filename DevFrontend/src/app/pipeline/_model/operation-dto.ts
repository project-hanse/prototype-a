export interface OperationDto {
  operationId: string;
  operationName: string;
  operationFullName: string;
  framework: string;
  description: string;
  operationInputType: OperationInputTypes;
}

export interface OperationDtoGroup {
  sectionTitle: string;
  operations: Array<OperationDto>;
}

export enum OperationInputTypes {
  File = 0,
  Single = 1,
  Double = 2,
  Unknown
}
