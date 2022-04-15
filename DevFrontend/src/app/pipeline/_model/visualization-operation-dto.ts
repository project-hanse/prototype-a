import {Node} from 'vis';
import {Dataset} from './dataset';

export interface VisualizationOperationDto extends Node {
	operationIdentifier: string;
	inputs: Array<Dataset>;
	outputs: Array<Dataset>;
}
