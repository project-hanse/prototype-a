import {PipelineVisualizationDto} from './pipeline-visualization.dto';

export interface AddOperationResponse {
	pipelineId: string;
	operationId: string;
	pipelineVisualizationDto: PipelineVisualizationDto;
}
