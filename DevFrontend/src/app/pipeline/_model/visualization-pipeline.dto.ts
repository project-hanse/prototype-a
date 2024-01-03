import {Edge} from 'vis-network';
import {VisualizationOperationDto} from './visualization-operation-dto';

export interface VisualizationPipelineDto {
	pipelineId: string;
	pipelineName: string;
	nodes: Array<VisualizationOperationDto>;
	edges: Array<Edge>;
}
