import {BaseRequest} from '../../core/_model/base-request';

export interface CreatePipelineFromTemplateRequest extends BaseRequest {
	templateId: string;
}
