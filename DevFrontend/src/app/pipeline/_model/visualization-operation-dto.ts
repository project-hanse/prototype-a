import {Node} from 'vis';
import {Dataset} from './dataset';

export interface VisualizationOperationDto extends Node {
	inputs: Array<Dataset>;
	output: Dataset;
}
