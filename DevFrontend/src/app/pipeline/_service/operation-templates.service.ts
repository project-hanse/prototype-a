import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {OperationTemplateGroup, OperationTemplate} from '../_model/operation-template';

@Injectable({
	providedIn: 'root'
})
export class OperationTemplatesService extends BaseHttpService {

	constructor(protected httpClient: HttpClient) {
		super('api/v1/operationTemplates', httpClient);
	}

	public getOperations(): Observable<Array<OperationTemplate>> {
		return super.get();
	}

	public getOperationsGroups(): Observable<Array<OperationTemplateGroup>> {
		return super.get('grouped');
	}
}
