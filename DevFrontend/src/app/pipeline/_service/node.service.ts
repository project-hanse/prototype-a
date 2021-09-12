import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {Observable} from 'rxjs';
import {AddNodeRequest} from '../_model/add-node-request';
import {AddNodeResponse} from '../_model/add-node-response';

@Injectable({
  providedIn: 'root'
})
export class NodeService extends BaseHttpService {

  constructor(private httpClient: HttpClient) {
    super('api/v1/node', httpClient);
  }

  public getBlockInputDatasets(pipelineId: string, nodeId: string): Observable<string[]> {
    return super.get(pipelineId + '/' + nodeId + '/datasets/input');
  }

  public addNode(addNodeRequest: AddNodeRequest): Observable<AddNodeResponse> {
    return this.httpClient.post<AddNodeResponse>(this.getUrl('add'), addNodeRequest);
  }
}
