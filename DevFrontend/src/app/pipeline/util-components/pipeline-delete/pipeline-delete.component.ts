import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {Subscription} from 'rxjs';
import {PipelineService} from '../../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-delete',
	templateUrl: './pipeline-delete.component.html',
	styleUrls: ['./pipeline-delete.component.scss']
})
export class PipelineDeleteComponent implements OnInit, OnDestroy {

	@Input() pipelineId?: string;
	@Input() compact: boolean = false;

	private readonly subscriptions: Subscription = new Subscription();

	constructor(private pipelineService: PipelineService, private router: Router, private route: ActivatedRoute) {
	}

	ngOnInit(): void {
	}


	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	deletePipeline(pipelineId: string): void {
		if (confirm('This pipeline will be permanently deleted. Are you sure?')) {
			this.subscriptions.add(
				this.pipelineService.deletePipeline(pipelineId).subscribe(
					(p) => {
						console.log('Deleted pipeline: ', p);
						this.router.navigate(['..'], {relativeTo: this.route});
					},
					err => {
						console.error('Error deleting pipeline: ', err);
					}
				)
			);
		}
	}

}
