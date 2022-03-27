import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {combineLatest, debounceTime, forkJoin, Observable, ReplaySubject, Subject, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {ModelService} from '../../admin/model/_service/model.service';
import {OperationIds} from '../../core/_model/operation-ids';
import {FileInfoDto} from '../../files/_model/file-info-dto';
import {FilesService} from '../../utils/_services/files.service';
import {AddOperationRequest} from '../_model/add-operation-request';
import {DatasetType} from '../_model/dataset';
import {OperationTemplate, OperationTemplateGroup} from '../_model/operation-template';
import {PipelineInfoDto} from '../_model/pipeline';
import {RemoveOperationsRequest} from '../_model/remove-operations-request';
import {VisualizationOperationDto} from '../_model/visualization-operation-dto';
import {VisualizationPipelineDto} from '../_model/visualization-pipeline.dto';
import {OperationTemplatesService} from '../_service/operation-templates.service';
import {OperationsService} from '../_service/operations.service';

@Component({
	selector: 'ph-pipeline-toolbox',
	templateUrl: './pipeline-toolbox.component.html',
	styleUrls: ['./pipeline-toolbox.component.scss']
})
export class PipelineToolboxComponent implements OnInit, OnDestroy {

	constructor(private operationsService: OperationTemplatesService,
							private nodeService: OperationsService,
							private filesService: FilesService,
							private modelService: ModelService) {
		this.subscriptions = new Subscription();
		this.pipelineChanged = new EventEmitter<VisualizationPipelineDto>();
	}

	@Input()
	public pipelineInfoDto?: PipelineInfoDto;

	@Input()
	public selectedOperationIds: Array<string> = [];

	@Input()
	public selectedOperations: Array<VisualizationOperationDto> = [];

	@Output()
	public readonly pipelineChanged: EventEmitter<VisualizationPipelineDto>;

	private $operationPredictions: Subject<string[]> = new ReplaySubject();
	private $operationSearchValues: Subject<string> = new ReplaySubject();
	private $selectedOperationIds: Subject<Array<VisualizationOperationDto>> = new ReplaySubject();
	private $operationTemplateGroups: Observable<Array<OperationTemplateGroup>>;
	private $userFiles?: Observable<Array<FileInfoDto>>;

	private readonly subscriptions: Subscription;
	operationsSearchText?: string;
	filesSearchText?: string;

	static isValidFileExtension(extension: string): boolean {
		return ['.csv', '.xlsx'].includes(extension);
	}

	private static getFileInputOperation(userFile: FileInfoDto): OperationTemplate {
		if (userFile.fileExtension === '.csv') {
			return {
				operationId: OperationIds.OpIdPdFileReadCsv,
				operationName: 'read_csv',
				operationFullName: 'Read CSV File',
				inputTypes: [DatasetType.File],
				outputType: DatasetType.PdDataFrame,
				framework: 'pandas',
				description: '',
				signature: '',
				defaultConfig: new Map<string, string>(),
				sectionTitle: ''
			};
		}
		return {
			operationId: OperationIds.OpIdPdFileReadExcel,
			operationName: 'read_excel',
			operationFullName: 'Read Excel File',
			framework: 'pandas',
			description: '',
			inputTypes: [DatasetType.File],
			outputType: DatasetType.PdDataFrame,
			signature: '',
			defaultConfig: new Map<string, string>(),
			sectionTitle: ''
		};
	}

	ngOnInit(): void {
		this.$operationTemplateGroups = combineLatest([
			this.operationsService.getOperations(),
			this.$operationSearchValues.pipe(
				debounceTime(250),
				map(value => value.trim()),
				map(value => value.toLowerCase()),
			),
			this.$selectedOperationIds.pipe(map(ids => ids ?? [])),
			this.$operationPredictions
		])
			.pipe(
				map(
					([operationTemplates, searchValue, selectedOperations, opPredictions]) => {
						// match required amount of input-datasets
						if (selectedOperations.length > 0) {
							// filter operation templates that require more or less input datasets than selected nodes
							operationTemplates = operationTemplates.filter(operation => {
								return operation.inputTypes?.length >= selectedOperations.length;
							});
						}

						// match required input-dataset types
						if (selectedOperations.length > 0) {
							operationTemplates = operationTemplates.filter(operation => {
								// TODO this could be change to be order independent, but this requires automatically changing the order of the input-datasets
								const selectedTypes = selectedOperations.map(o => o.output.type);
								for (let i = 0; i < selectedTypes.length; i++) {
									if (selectedTypes[i] !== operation.inputTypes[i]) {
										// don't show this operation template if any of the selected type types does not match the input types of the operation template
										return false;
									}
								}
								return true;
							});
						}

						if (searchValue.length > 0) {
							operationTemplates = operationTemplates.filter(operation => {
								const searchIn = [operation.operationName, operation.operationFullName, operation.description, operation.sectionTitle];
								const searchInText = searchIn.join('').replace(' ', '').toLowerCase();
								return searchInText.includes(this.operationsSearchText.replace(' ', '').toLowerCase());
							});
						}

						if (opPredictions.length > 0) {
							operationTemplates.sort((operationA, operationB) => {
								const combinedIdA = this.getCombinedId(operationA);
								const combinedIdB = this.getCombinedId(operationB);
								let indexA = opPredictions.indexOf(combinedIdA);
								let indexB = opPredictions.indexOf(combinedIdB);
								if (indexA === -1 && indexB === -1) {
									return 0;
								}
								indexA = indexA === -1 ? Number.MAX_SAFE_INTEGER : indexA;
								indexB = indexB === -1 ? Number.MAX_SAFE_INTEGER : indexB;
								return indexA - indexB;
							});
						}
						return operationTemplates;
					}),
				map(operationTemplates => {
					// group operationTemplates by sectionTitle to OperationTemplateGroups
					return operationTemplates.reduce((groups, operationTemplate) => {
						const group = groups.find(grp => grp.sectionTitle === operationTemplate.sectionTitle);
						if (group) {
							group.operations.push(operationTemplate);
						} else {
							groups.push({
								sectionTitle: operationTemplate.sectionTitle,
								operations: [operationTemplate]
							});
						}
						return groups;
					}, []);
				}),
				map(operationTemplateGroups => operationTemplateGroups.filter(opg => opg.operations.length !== 0))
			);
		this.$operationSearchValues.next('');
		this.$selectedOperationIds.next([]);
		this.$operationPredictions.next([]);
	}

	private getCombinedId(operation: OperationTemplate): string {
		return `${operation.operationId}-${operation.operationName}`.toLowerCase();
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	getOperationTemplates(): Observable<Array<OperationTemplateGroup>> {
		return this.$operationTemplateGroups;
	}

	onAddNode(operation: OperationTemplate): void {
		const request: AddOperationRequest = {
			pipelineId: this.pipelineInfoDto.id,
			operationTemplate: operation,
			predecessorOperationIds: this.selectedOperationIds
		};
		this.subscriptions.add(
			this.nodeService.addOperation(request).subscribe(
				response => {
					this.pipelineChanged.emit(response.pipelineVisualizationDto);
				},
				error => {
					console.error('Failed to add node', error);
				})
		);
	}

	onRemoveNodes(): void {
		const request: RemoveOperationsRequest = {
			pipelineId: this.pipelineInfoDto.id,
			operationIdsToBeRemoved: this.selectedOperationIds
		};
		this.subscriptions.add(
			this.nodeService.removeOperations(request).subscribe(
				response => {
					this.pipelineChanged.emit(response.pipelineVisualizationDto);
				},
				error => {
					console.error('Failed to add node', error);
				})
		);
	}

	addFile(userFile: FileInfoDto): void {
		if (!PipelineToolboxComponent.isValidFileExtension(userFile.fileExtension)) {
			return;
		}
		const request: AddOperationRequest = {
			pipelineId: this.pipelineInfoDto.id,
			predecessorOperationIds: [],
			operationTemplate: PipelineToolboxComponent.getFileInputOperation(userFile),
			options: {
				objectBucket: userFile.bucketName,
				objectKey: userFile.objectKey
			}
		};
		this.subscriptions.add(
			this.nodeService.addOperation(request).subscribe(
				response => {
					this.pipelineChanged.emit(response.pipelineVisualizationDto);
				},
				error => {
					console.error('Failed to add file', error);
				}
			)
		);
	}

	getUserFiles(): Observable<Array<FileInfoDto>> {
		if (!this.$userFiles) {
			this.$userFiles = forkJoin([this.filesService.getUserFileInfos(), this.filesService.getDefaultFileInfos()])
				.pipe(
					map(files => {
						return files[0].concat(files[1]);
					})
				);
		}
		return this.$userFiles;
	}

	showFile(userFile: FileInfoDto): boolean {
		if (!this.filesSearchText) {
			return true;
		}
		if (!userFile?.fileName) {
			return true;
		}
		if (!PipelineToolboxComponent.isValidFileExtension(userFile.fileExtension)) {
			return false;
		}
		return userFile.fileName.toLowerCase().includes(this.filesSearchText.toLowerCase());
	}

	onSearchTextChange(value: string): void {
		this.operationsSearchText = value;
		this.$operationSearchValues.next(value);
	}

	public setSelectedOperations(selectedOps: Array<VisualizationOperationDto>): void {
		this.selectedOperationIds = selectedOps.map(op => op.id as string);
		this.$selectedOperationIds.next(selectedOps);
		this.subscriptions.add(forkJoin(selectedOps.map(op => {
				return this.modelService.loadPrediction({
					input_0_dataset_type: op.inputs[0]?.type,
					input_1_dataset_type: op.inputs[1]?.type,
					input_2_dataset_type: op.inputs[2]?.type,
					feat_pred_count: op.inputs?.length ?? 0,
					feat_pred_id: op.operationIdentifier,
				});
			})).pipe(
				debounceTime(50),
				map((predictions) => {
					return new Array(...(new Set(...predictions)));
				})
			).subscribe(
				pred => this.$operationPredictions.next(pred),
				err => console.error(err)),
		);
	}
}
