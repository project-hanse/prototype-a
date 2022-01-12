import {VisualizationPipelineDto} from './visualization-pipeline.dto';

export interface RemoveOperationsResponse {
	pipelineId: string;
	pipelineVisualizationDto: VisualizationPipelineDto;
}
