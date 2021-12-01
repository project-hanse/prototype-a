import {BaseRequest} from '../../core/_model/base-request';

export interface RemoveOperationsRequest extends BaseRequest {
	pipelineId: string;
	nodeIdsToBeRemoved: Array<string>;
}
