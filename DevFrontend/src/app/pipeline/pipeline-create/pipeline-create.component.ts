import {Component, OnDestroy, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {Observable, Subscription} from 'rxjs';
import {PipelineInfoDto} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-create',
	templateUrl: './pipeline-create.component.html',
	styleUrls: ['./pipeline-create.component.scss']
})
export class PipelineCreateComponent implements OnInit, OnDestroy {
	$templates: Observable<Array<PipelineInfoDto>>;

	selectedTemplate?: PipelineInfoDto;

	private readonly subscriptions: Subscription = new Subscription();
	loading: boolean = false;

	constructor(private pipelineService: PipelineService, private router: Router) {
		this.$templates = this.pipelineService.getPipelineTemplates();
	}

	ngOnInit(): void {
	}


	ngOnDestroy(): void {
		this.subscriptions.add(this.subscriptions);
	}

	createButtonClicked(selectedTemplate: PipelineInfoDto): void {
		this.loading = true;
		this.subscriptions.add(
			this.pipelineService.createFromTemplate({templateId: selectedTemplate.id}).subscribe(
				success => {
					this.router.navigate(['/pipeline', success.pipelineId]);
					this.loading = false;
				},
				error => {
					console.error(error);
					this.loading = false;
				}
			)
		);
	}
}
