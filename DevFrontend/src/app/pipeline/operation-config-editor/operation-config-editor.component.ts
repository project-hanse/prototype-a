import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {OperationsService} from '../_service/operations.service';

@Component({
	selector: 'ph-operation-config-editor',
	templateUrl: './operation-config-editor.component.html',
	styleUrls: ['./operation-config-editor.component.scss']
})
export class OperationConfigEditorComponent implements OnInit, OnDestroy {
	@Input()
	pipelineId?: string;
	operationIds?: Array<string>;
	$configs: Array<Observable<Map<string, string>>> = [];

	private readonly subscriptions: Subscription;

	constructor(private operationsService: OperationsService) {
		this.subscriptions = new Subscription();
	}

	ngOnInit(): void {
	}

	getConfig(operationId: string): Observable<Map<string, string>> {
		if (!this.$configs[operationId]) {
			this.$configs[operationId] = this.operationsService.getConfig(this.pipelineId, operationId);
		}
		return this.$configs[operationId];
	}

	onSubmit(operationId: string, config: Map<string, string>): void {
		this.subscriptions.add(
			this.operationsService.updateConfig(this.pipelineId, operationId, config).subscribe(
				res => console.log(res),
				err => console.error(err)
			)
		);
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}
}
