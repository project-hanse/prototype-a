import {BaseRequest} from '../../core/_model/base-request';

export interface CreateFromTemplateRequest extends BaseRequest {
	templateId: string;
}
