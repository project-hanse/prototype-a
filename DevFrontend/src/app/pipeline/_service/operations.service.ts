import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {AddOperationRequest} from '../_model/add-operation-request';
import {AddOperationResponse} from '../_model/add-operation-response';
import {RemoveOperationsRequest} from '../_model/remove-operations-request';
import {RemoveOperationsResponse} from '../_model/remove-operations-response';

@Injectable({
	providedIn: 'root'
})
export class OperationsService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('api/v1/operations', httpClient);
	}

	public getBlockInputDatasets(pipelineId: string, operationId: string): Observable<string[]> {
		return super.get(pipelineId + '/' + operationId + '/datasets/input');
	}

	public addOperation(request: AddOperationRequest): Observable<AddOperationResponse> {
		return this.httpClient.post<AddOperationResponse>(this.getPipelinesUrl('add'), request);
	}

	public removeOperations(request: RemoveOperationsRequest): Observable<RemoveOperationsResponse> {
		return this.httpClient.post<RemoveOperationsResponse>(this.getPipelinesUrl('remove'), request);
	}

	public getOutputKey(pipelineId: string, operationId: string): Observable<{ key: string }> {
		return this.httpClient.get<{ key: string }>(this.getPipelinesUrl(pipelineId, operationId, 'output-key'));
	}

	public getPreviewHtml(outputKey: string): Observable<string> {
		return this.httpClient.get(`${environment.datasetApi}/api/datasets/hash/describe/html/${outputKey}`, {responseType: 'text'});
	}

	public getConfig(pipelineId: string, operationId: string): Observable<Map<string, string>> {
		return this.httpClient.get<Map<string, string>>(this.getPipelinesUrl(pipelineId, operationId, 'config'));
	}

	public updateConfig(pipelineId: string, operationId: string, config: Map<string, string>): Observable<any> {
		return this.httpClient.post(this.getPipelinesUrl(pipelineId, operationId, 'config'), config);
	}
}
