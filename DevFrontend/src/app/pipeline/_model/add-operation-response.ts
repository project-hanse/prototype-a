import {VisualizationPipelineDto} from './visualization-pipeline.dto';

export interface AddOperationResponse {
	pipelineId: string;
	operationId: string;
	pipelineVisualizationDto: VisualizationPipelineDto;
}
