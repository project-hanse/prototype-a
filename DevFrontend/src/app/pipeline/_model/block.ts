import {BasePersistentModel} from './base-persistent-model';

export interface Block extends BasePersistentModel {
  pipelineId: string;
  successors: Block[];
  operation: string;
  operationConfiguration: Map<string, string>;
  includeInHash: string;
}
