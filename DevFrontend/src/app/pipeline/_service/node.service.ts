import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {AddNodeRequest} from '../_model/add-node-request';
import {AddNodeResponse} from '../_model/add-node-response';
import {RemoveNodesRequest} from '../_model/remove-nodes-request';
import {RemoveNodesResponse} from '../_model/remove-nodes-response';

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
		return this.httpClient.post<AddNodeResponse>(this.getPipelinesUrl('add'), addNodeRequest);
	}

	public removeNodes(request: RemoveNodesRequest): Observable<RemoveNodesResponse> {
		return this.httpClient.post<RemoveNodesResponse>(this.getPipelinesUrl('remove'), request);
	}

	public getResultHash(pipelineId: string, nodeId: string): Observable<{ hash: string }> {
		return this.httpClient.get<{ hash: string }>(this.getPipelinesUrl(pipelineId, nodeId, 'result-hash'));
	}

	public getPreviewHtml(hash: string): Observable<string> {
		return this.httpClient.get(`${environment.datasetApi}/api/datasets/hash/describe/html/${hash}`, {responseType: 'text'});
	}

	public getConfig(pipelineId: string, nodeId: string): Observable<Map<string, string>> {
		return this.httpClient.get<Map<string, string>>(this.getPipelinesUrl(pipelineId, nodeId, 'config'));
	}

	public updateConfig(pipelineId: string, nodeId: string, config: Map<string, string>): Observable<any> {
		return this.httpClient.post(this.getPipelinesUrl(pipelineId, nodeId, 'config'), config);
	}
}
