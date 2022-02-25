import {Component, OnDestroy, OnInit} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {BaseResponse} from '../../core/_model/base-response';
import {Pipeline} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-list-view',
	templateUrl: './pipeline-list-view.component.html',
	styleUrls: ['./pipeline-list-view.component.scss']
})
export class PipelineListViewComponent implements OnInit, OnDestroy {

	private $pipelines: Observable<Pipeline[]>;
	private readonly subscriptions: Subscription;

	uploadFunction = (formData: FormData) => {
		return this.pipelineService.importPipeline(formData);
	}

	constructor(private pipelineService: PipelineService) {
		this.subscriptions = new Subscription();
	}

	ngOnInit(): void {
	}

	public getPipelines(): Observable<Pipeline[]> {
		if (!this.$pipelines) {
			this.$pipelines = this.pipelineService.getPipelines();
		}
		return this.$pipelines;
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	onPipelineImported(response: BaseResponse): void {
		this.$pipelines = undefined;
	}
}
