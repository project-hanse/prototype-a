import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {PipelineCandidate} from '../../admin/pipeline-candidates/_model/pipeline-candidate';
import {PaginatedList} from '../../core/_model/paginated-list';
import {Pagination} from '../../core/_model/pagination';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {CreatePipelineFromTemplateRequest} from '../_model/create-pipeline-from-template-request';
import {CreatePipelineFromTemplateResponse} from '../_model/create-pipeline-from-template-response';
import {ImportPipelineResponse} from '../_model/import-pipeline-response';
import {PipelineInfoDto} from '../_model/pipeline';
import {VisualizationPipelineDto} from '../_model/visualization-pipeline.dto';

@Injectable({
	providedIn: 'root'
})
export class PipelineService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('api/v1/pipelines', httpClient);
	}

	public getPipelineDtos(pagination: Pagination, userIdentifier?: string): Observable<PaginatedList<PipelineInfoDto>> {
		const params: any = {...pagination};
		if (userIdentifier) {
			params.userIdentifier = userIdentifier;
		}
		return this.httpClient.get<PaginatedList<PipelineInfoDto>>(this.getPipelinesUrl(''), {params});
	}

	public getPipelineDto(id: string): Observable<PipelineInfoDto> {
		return super.get(id);
	}

	public update(pipeline: PipelineInfoDto): Observable<PipelineInfoDto> {
		return this.httpClient.post<PipelineInfoDto>(this.getPipelinesUrl(pipeline.id), pipeline);
	}

	public generateNew(): Observable<number> {
		return super.get('create/defaults');
	}

	public executePipeline(id: string): Observable<string> {
		return super.get('execute/' + id);
	}

	public getPipelineForVisualization(pipelineId: string): Observable<VisualizationPipelineDto> {
		return super.get<VisualizationPipelineDto>(`vis/${pipelineId}`);
	}

	public getPipelineTemplates(): Observable<Array<PipelineInfoDto>> {
		return super.get('templates');
	}

	public createFromTemplate(request: CreatePipelineFromTemplateRequest): Observable<CreatePipelineFromTemplateResponse> {
		return this.httpClient.post<CreatePipelineFromTemplateResponse>(this.getPipelinesUrl('create', 'from', 'template'), request);
	}

	public getPipelineDownloadLink(id: string): string {
		return this.getPipelinesUrl('export', id);
	}

	public importPipeline(formData: FormData): Observable<ImportPipelineResponse> {
		return this.httpClient.post<ImportPipelineResponse>(this.getPipelinesUrl('import'), formData);
	}

	public deletePipeline(pipelineId: string): Observable<PipelineInfoDto> {
		return this.httpClient.delete<PipelineInfoDto>(this.getPipelinesUrl(pipelineId));
	}

	public deletePipelines(pipelineIds: string[]): Observable<number> {
		return this.httpClient.delete<number>(this.getPipelinesUrl(), {params: {pipelineIds}});
	}

	public enqueuePipelines(pipelineIds: string[]): Observable<Array<string>> {
		return this.httpClient.post<Array<string>>(this.getPipelinesUrl('execute'), pipelineIds);
	}

	public getPipelineCandidates(pagination: Pagination): Observable<PaginatedList<PipelineCandidate>> {
		return this.httpClient.get<PaginatedList<PipelineCandidate>>(this.getPipelinesUrl('candidate'), {params: {...pagination}});
	}

	public importPipelineCandidate(pipelineCandidateId: string, deleteAfterImport: boolean = false, username: string = null): Observable<string> {
		return this.httpClient.get<string>(this.getPipelinesUrl('candidate', 'import', pipelineCandidateId), {
			params: {
				deleteAfterImport,
				username
			}
		});
	}

	public processCandidates(candidateIds: string[]): Observable<number> {
		return this.httpClient.post<number>(this.getPipelinesUrl('candidate', 'process'), candidateIds);
	}
}
