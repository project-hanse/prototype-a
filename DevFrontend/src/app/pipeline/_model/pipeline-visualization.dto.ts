import {Edge, Node} from 'vis';

export interface PipelineVisualizationDto {
	pipelineId: string;
	pipelineName: string;
	nodes: Array<Node>;
	edges: Array<Edge>;
}
