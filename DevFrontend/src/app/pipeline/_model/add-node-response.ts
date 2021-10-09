import {PipelineVisualizationDto} from './pipeline-visualization.dto';

export interface AddNodeResponse {
  pipelineId: string;
  nodeId: string;
  pipelineVisualizationDto: PipelineVisualizationDto;
}
