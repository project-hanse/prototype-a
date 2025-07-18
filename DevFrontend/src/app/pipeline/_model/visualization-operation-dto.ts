import {Node} from 'vis-network';
import {Dataset, DatasetView} from './dataset';

export interface VisualizationOperationDto extends Node {
	operationId: string;
	operationTemplateId: string;
	operationName: string;
	operationIdentifier: string;
	inputs: Array<Dataset>;
	outputs: Array<DatasetView>;
}
