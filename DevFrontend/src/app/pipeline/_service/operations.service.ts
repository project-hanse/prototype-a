import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {OperationDto, OperationDtoGroup} from '../_model/operation-dto';

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

	public getOperationsGroups(): Observable<Array<OperationDtoGroup>> {
		return super.get('grouped');
	}
}
