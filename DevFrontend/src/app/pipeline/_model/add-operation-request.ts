import {BaseRequest} from '../../core/_model/base-request';
import {Dataset} from './dataset';
import {OperationTemplate} from './operation-template';

export interface AddOperationRequest extends BaseRequest {
	pipelineId: string;
	predecessorOperationDtos: Array<PredecessorOperationDto>;
	newOperationTemplate: OperationTemplate;
	options?: any;
}

export interface PredecessorOperationDto {
	operationId?: string;
	operationTemplateId?: string;
	outputDatasets: Array<Dataset>;
}
