import {PipelineVisualizationDto} from './pipeline-visualization.dto';

export interface RemoveNodesResponse {
	pipelineId: string;
	pipelineVisualizationDto: PipelineVisualizationDto;
}
