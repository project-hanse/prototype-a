import {BaseRequest} from '../../core/_model/base-request';
import {OperationTemplate} from './operation-template';

export interface AddOperationRequest extends BaseRequest {
	pipelineId: string;
	predecessorOperationIds: Array<string>;
	operationTemplate: OperationTemplate;
	options?: any;
}
