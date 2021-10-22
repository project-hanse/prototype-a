import {BasePersistentModel} from './base-persistent-model';

export interface PipelineNode extends BasePersistentModel {
	pipelineId: string;
	successors: PipelineNode[];
	operation: string;
	operationConfiguration: Map<string, string>;
	includeInHash: string;
}
