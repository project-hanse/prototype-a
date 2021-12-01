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

	private $hashes = {};
	private $previewHtml = {};

	constructor(private operationsService: OperationsService) {
	}

	ngOnInit(): void {
	}

	getHash(operationId: string): Observable<{ hash: string }> {
		if (!this.$hashes[operationId]) {
			this.$hashes[operationId] = this.operationsService.getResultHash(this.pipelineId, operationId);
		}
		return this.$hashes[operationId];
	}

	getPreviewHtml(hash: string): Observable<string> {
		if (!this.$previewHtml[hash]) {
			this.$previewHtml[hash] = this.operationsService.getPreviewHtml(hash);
		}
		return this.$previewHtml[hash];
	}

	getDatasetLink(hash: string): string {
		return `${environment.datasetApi}/api/datasets/html/${hash}`;
	}
}
