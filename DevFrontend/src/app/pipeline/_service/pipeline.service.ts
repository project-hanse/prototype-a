import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Pipeline} from '../_model/pipeline';
import {BaseHttpService} from '../../core/_service/base-http.service';

@Injectable({
  providedIn: 'root'
})
export class PipelineService extends BaseHttpService {

  constructor(private httpClient: HttpClient) {
    super('api/v1/pipeline', httpClient);
  }

  public getPipelines(): Observable<Pipeline[]> {
    return super.get();
  }

  public getPipeline(id: string): Observable<Pipeline> {
    return super.get(id);
  }

  public generateNew(): Observable<number> {
    return super.get('create/defaults');
  }

  public executePipeline(id: string): Observable<string> {
    return super.get('execute/' + id);
  }
}
