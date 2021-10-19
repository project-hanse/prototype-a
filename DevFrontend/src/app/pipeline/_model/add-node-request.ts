import {BaseRequest} from '../../core/_model/base-request';
import {OperationDto} from './operation-dto';

export interface AddNodeRequest extends BaseRequest {
	pipelineId: string;
	predecessorNodeIds: Array<string>;
	operation: OperationDto;
}
