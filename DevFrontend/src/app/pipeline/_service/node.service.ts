import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {Observable} from 'rxjs';

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
}
