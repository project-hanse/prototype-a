import {Component, Input, OnInit} from '@angular/core';
import {DomSanitizer} from '@angular/platform-browser';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {environment} from '../../../environments/environment';
import {FilesService} from '../../utils/_services/files.service';
import {Dataset, DatasetType} from '../_model/dataset';
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

	private $datasetOutputs = {};
	private $previewHtml = {};
	private $plotOutputs = {};

	constructor(private operationsService: OperationsService, private filesService: FilesService, private domSanitizer: DomSanitizer) {
	}

	ngOnInit(): void {
	}

	getOutputDataset(operationId: string): Observable<Dataset> {
		if (!this.$datasetOutputs[operationId]) {
			this.$datasetOutputs[operationId] = this.operationsService.getOutputDataset(this.pipelineId, operationId);
		}
		return this.$datasetOutputs[operationId];
	}

	getPreviewHtml(datasetKey: string): Observable<string> {
		if (!this.$previewHtml[datasetKey]) {
			this.$previewHtml[datasetKey] = this.operationsService.getPreviewHtml(datasetKey);
		}
		return this.$previewHtml[datasetKey];
	}


	getPlot(output: Dataset): Observable<string> {
		if (!this.$plotOutputs[output.key]) {
			this.$plotOutputs[output.key] = this.filesService.getPlot(output).pipe(map(svg => {
				console.log(svg);
				return this.domSanitizer.bypassSecurityTrustHtml(svg);
			}));
		}
		return this.$plotOutputs[output.key];
	}

	getDatasetLink(hash: string): string {
		return `${environment.datasetApi}/api/datasets/html/${hash}`;
	}

	isDataframe(output: Dataset): boolean {
		return output.type === DatasetType.PdDataFrame;
	}

	isPlot(output: Dataset): boolean {
		return output.type === DatasetType.StaticPlot;
	}

	getPlotUrl(output: Dataset): string {
		return `${environment.filesApi}/api/v1/files/plot?store=${output.store}&key=${output.key}`;
	}
}
