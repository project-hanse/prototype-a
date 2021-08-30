import {Node, Edge} from 'vis-network';

export interface PipelineVisualizationDto {
  pipelineId: string;
  pipelineName: string;
  nodes: Array<Node>;
  edges: Array<Edge>;
}
