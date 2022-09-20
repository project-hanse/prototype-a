import {Component, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {Pipeline, PipelineInfoDto} from '../_model/pipeline';
import {OperationsService} from '../_service/operations.service';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-editor',
	templateUrl: './pipeline-editor.component.html',
	styleUrls: ['./pipeline-editor.component.scss']
})
export class PipelineEditorComponent implements OnInit, OnDestroy {

	private readonly subscriptions: Subscription;

	$pipelineId: Observable<string>;
	private $pipeline: Observable<PipelineInfoDto>;
	private $rootInputDatasets: Observable<string[]>;
	lastSelectedNodeIds?: Array<string>;

	constructor(private route: ActivatedRoute, private pipelineService: PipelineService, private nodeService: OperationsService) {
		this.subscriptions = new Subscription();
		this.$pipelineId = this.route.paramMap.pipe(map(p => p.get('id')));
	}

	ngOnInit(): void {
	}

	public getPipeline(id: string): Observable<PipelineInfoDto> {
		if (!this.$pipeline) {
			this.$pipeline = this.pipelineService.getPipelineDto(id);
		}
		return this.$pipeline;
	}

	public executePipeline(id: string, allowResultCaching: boolean): void {
		this.subscriptions.add(
			this.pipelineService.executePipeline(id, allowResultCaching)
				.subscribe(
					executionId => console.log('Started execution' + executionId)
				)
		);
	}

	public getRootInputDatasets(pipeline: Pipeline): Observable<string[]> {
		if (!this.$rootInputDatasets) {
			this.$rootInputDatasets = this.nodeService.getBlockInputDatasets(pipeline.id, pipeline.root[0].id);
		}
		return this.$rootInputDatasets;
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}
}
