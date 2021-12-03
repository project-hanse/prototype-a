import {BasePersistentModel} from './base-persistent-model';

export interface Operation extends BasePersistentModel {
	pipelineId: string;
	successors: Operation[];
	operation: string;
	operationConfiguration: Map<string, string>;
	includeInHash: string;
}
