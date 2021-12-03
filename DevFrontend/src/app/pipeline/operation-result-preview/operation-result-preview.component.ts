import {Component, Input, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';
import {OperationsService} from '../_service/operations.service';

@Component({
	selector: 'ph-operation-result-preview',
	templateUrl: './operation-result-preview.component.html',
	styleUrls: ['./operation-result-preview.component.scss']
})
export class OperationResultPreviewComponent implements OnInit {

	@Input()
	pipelineId?: string;
	operationIds?: Array<string>;

	private $datasetKeys = {};
	private $previewHtml = {};

	constructor(private operationsService: OperationsService) {
	}

	ngOnInit(): void {
	}

	getOutputKey(operationId: string): Observable<{ key: string }> {
		if (!this.$datasetKeys[operationId]) {
			this.$datasetKeys[operationId] = this.operationsService.getOutputKey(this.pipelineId, operationId);
		}
		return this.$datasetKeys[operationId];
	}

	getPreviewHtml(datasetKey: string): Observable<string> {
		if (!this.$previewHtml[datasetKey]) {
			this.$previewHtml[datasetKey] = this.operationsService.getPreviewHtml(datasetKey);
		}
		return this.$previewHtml[datasetKey];
	}

	getDatasetLink(hash: string): string {
		return `${environment.datasetApi}/api/datasets/html/${hash}`;
	}
}
