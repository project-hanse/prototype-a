import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {Subscription} from 'rxjs';
import {PipelineInfoDto} from '../../pipeline/_model/pipeline';
import {PipelineService} from '../../pipeline/_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-title',
	templateUrl: './pipeline-title.component.html',
	styleUrls: ['./pipeline-title.component.scss']
})
export class PipelineTitleComponent implements OnInit, OnDestroy {

	@Input()
	pipeline?: PipelineInfoDto;

	editMode: boolean = false;

	private readonly subscriptions: Subscription = new Subscription();

	constructor(private pipelineService: PipelineService) {
	}

	ngOnInit(): void {
	}

	nameClicked(): void {
		this.editMode = true;
	}

	onSubmit($event: Event, pipeline: PipelineInfoDto): void {
		this.subscriptions.add(
			this.pipelineService.update(pipeline)
				.subscribe(
					response => {
						this.editMode = false;
						this.pipeline = response;
					},
					err => {
						this.editMode = false;
						console.log(err);
					}
				));
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}
}
