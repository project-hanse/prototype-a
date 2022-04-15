import {DatasetType} from './dataset';

export interface OperationTemplate {
	operationId: string;
	operationName: string;
	operationFullName: string;
	inputTypes: Array<DatasetType>;
	outputTypes: Array<DatasetType>;
	framework: string;
	description: string;
	signature: string;
	defaultConfig: Map<string, string>;
	sectionTitle: string;
}

export interface OperationTemplateGroup {
	sectionTitle: string;
	operations: Array<OperationTemplate>;
}
