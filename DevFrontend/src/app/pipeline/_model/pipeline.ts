import {BasePersistentModel} from './base-persistent-model';
import {Operation} from './operation';

export interface Pipeline extends BasePersistentModel {
	name: string;
	root: Operation[];
	userIdentifier: string;
}

export interface PipelineInfoDto {
	id?: string;
	name: string;
	createdOn?: Date | string;
	successfullyExecutable: boolean;
	lastRunStart?: Date;
	lastRunSuccess?: Date;
	lastRunFailure?: Date;
}
