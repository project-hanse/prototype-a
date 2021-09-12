import {BaseRequest} from '../../core/_model/base-request';

export interface RemoveNodesRequest extends BaseRequest {
  pipelineId: string;
  nodeIdsToBeRemoved: Array<string>;
}
