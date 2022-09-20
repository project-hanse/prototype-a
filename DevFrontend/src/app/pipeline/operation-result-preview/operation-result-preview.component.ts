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

	getOutputDatasets(operationId: string): Observable<Array<Dataset>> {
		if (!this.$datasetOutputs[operationId]) {
			this.$datasetOutputs[operationId] = this.operationsService.getOutputDataset(this.pipelineId, operationId);
		}
		return this.$datasetOutputs[operationId];
	}

	getPreviewHtml(dataset: Dataset): Observable<string> {
		if (!this.$previewHtml[dataset.key]) {
			this.$previewHtml[dataset.key] = this.operationsService.getPreviewHtml(dataset.key);
		}
		return this.$previewHtml[dataset.key];
	}

	getDatasetName(dataset: Dataset): string {
		switch (dataset.type) {
			case DatasetType.PdDataFrame:
				return 'Dataframe';
			case DatasetType.StaticPlot:
				return 'Plot';
			case DatasetType.PdSeries:
				return 'Series';
			case DatasetType.Prophet:
				return 'Prophet';
			case DatasetType.SklearnModel:
				return 'Model';
			case DatasetType.SklearnEncoder:
				return 'Encoder';
			default:
				return 'Dataset';
		}
	}

	getDatasetLink(dataset: Dataset): string {
		switch (dataset.type) {
			case DatasetType.PdDataFrame:
				return `${environment.datasetApi}/api/dataframe/key/${dataset.key}?format=html`;
			case DatasetType.StaticPlot:
				return `${environment.filesApi}/api/v1/files/plotUrl?store=${dataset.store}&key=${dataset.key}`;
			case DatasetType.PdSeries:
				return `${environment.datasetApi}/api/series/key/${dataset.key}?format=html`;
			case DatasetType.Prophet:
				return `${environment.datasetApi}/api/string/key/${dataset.key}`;
			case DatasetType.SklearnModel:
				return `${environment.datasetApi}/api/string/key/${dataset.key}`;
			case DatasetType.SklearnEncoder:
				return `${environment.datasetApi}/api/string/key/${dataset.key}`;
			default:
				return '';
		}
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

	getMetadataLink(output: Dataset): string {
		switch (output.type) {
			case DatasetType.StaticPlot:
				return this.getPlotUrl(output);
			default:
				return `${environment.datasetApi}/api/metadata/key/${output.key}`;
		}
	}
}
