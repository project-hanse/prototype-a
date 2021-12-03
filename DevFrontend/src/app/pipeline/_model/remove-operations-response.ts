import {PipelineVisualizationDto} from './pipeline-visualization.dto';

export interface RemoveOperationsResponse {
	pipelineId: string;
	pipelineVisualizationDto: PipelineVisualizationDto;
}
