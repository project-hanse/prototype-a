import {Injectable} from '@angular/core';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {OperationDto} from '../_model/operation-dto';

@Injectable({
  providedIn: 'root'
})
export class OperationsService extends BaseHttpService {

  constructor(protected httpClient: HttpClient) {
    super('api/v1/operations', httpClient);
  }

  public getOperations(): Observable<Array<OperationDto>> {
    return super.get();
  }
}
