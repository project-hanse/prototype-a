import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {CreatePipelineFromTemplateRequest} from '../_model/create-pipeline-from-template-request';
import {CreatePipelineFromTemplateResponse} from '../_model/create-pipeline-from-template-response';
import {Pipeline, PipelineInfoDto} from '../_model/pipeline';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';

@Injectable({
	providedIn: 'root'
})
export class PipelineService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('api/v1/pipelines', httpClient);
	}

	public getPipelines(): Observable<Pipeline[]> {
		return super.get();
	}

	public getPipelineDto(id: string): Observable<PipelineInfoDto> {
		return super.get(id);
	}

	public generateNew(): Observable<number> {
		return super.get('create/defaults');
	}

	public executePipeline(id: string): Observable<string> {
		return super.get('execute/' + id);
	}

	public getPipelineForVisualization(pipelineId: string): Observable<PipelineVisualizationDto> {
		return super.get<PipelineVisualizationDto>(`vis/${pipelineId}`);
	}

	public getPipelineTemplates(): Observable<Array<PipelineInfoDto>> {
		return super.get('templates');
	}

	public createFromTemplate(request: CreatePipelineFromTemplateRequest): Observable<CreatePipelineFromTemplateResponse> {
		return this.httpClient.post<CreatePipelineFromTemplateResponse>(this.getPipelinesUrl('create', 'from', 'template'), request);
	}
}
