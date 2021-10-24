import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {Pipeline, PipelineInfoDto} from '../_model/pipeline';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';

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
}
