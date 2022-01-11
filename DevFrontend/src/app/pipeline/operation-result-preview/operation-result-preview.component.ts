import {Component, Input, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
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
	private plotUrls = {};

	constructor(private operationsService: OperationsService, private filesService: FilesService) {
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

	getDatasetLink(hash: string): string {
		return `${environment.datasetApi}/api/dataframe/html/${hash}`;
	}

	isDataframe(output: Dataset): boolean {
		return output.type === DatasetType.PdDataFrame;
	}

	isPlot(output: Dataset): boolean {
		return output.type === DatasetType.StaticPlot;
	}

	getPlotUrl(dataset: Dataset): string {
		return this.filesService.getPlotUrl(dataset);
		// return `${environment.filesApi}/api/v1/files/plotUrl?store=${dataset.store}&key=${dataset.key}`;
	}
}
