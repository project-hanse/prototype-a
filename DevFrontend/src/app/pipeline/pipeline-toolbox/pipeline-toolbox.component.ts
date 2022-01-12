import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {OperationIds} from '../../core/_model/operation-ids';
import {FileInfoDto} from '../../files/_model/file-info-dto';
import {FilesService} from '../../utils/_services/files.service';
import {AddOperationRequest} from '../_model/add-operation-request';
import {DatasetType} from '../_model/dataset';
import {OperationTemplate, OperationTemplateGroup} from '../_model/operation-template';
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
							private filesService: FilesService) {
		this.subscriptions = new Subscription();
		this.pipelineChanged = new EventEmitter<VisualizationPipelineDto>();
	}

	@Input()
	public subtitle?: string;

	@Input()
	public pipelineId: string;

	@Input()
	public selectedOperationIds: Array<string> = [];

	@Input()
	public selectedOperations: Array<VisualizationOperationDto> = [];

	@Output()
	public readonly pipelineChanged: EventEmitter<VisualizationPipelineDto>;

	private $operationTemplateGroups?: Observable<Array<OperationTemplateGroup>>;
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
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	getOperationTemplates(): Observable<Array<OperationTemplateGroup>> {
		if (!this.$operationTemplateGroups) {
			this.$operationTemplateGroups = this.operationsService.getOperationsGroups()
				.pipe(
					map(operationGroups => {
						for (const opGroup of operationGroups) {
							// Do not display operations that accept files
							// opGroup.operations = opGroup.operations.filter(operation => operation.inputTypes.includes(DatasetType.File));
						}
						return operationGroups;
					}),
				);
		}
		return this.$operationTemplateGroups;
	}

	showInAvailable(operation: OperationTemplate): boolean {
		if (this.operationsSearchText) {
			const searchIn = [operation.operationName, operation.operationFullName, operation.description, operation.sectionTitle];
			const searchInText = searchIn.join('').replace(' ', '').toLowerCase();
			if (!searchInText.includes(this.operationsSearchText.replace(' ', '').toLowerCase())) {
				return false;
			}
		}
		if (this.selectedOperationIds.length === 0) {
			return true;
		}
		if (operation.inputTypes?.length < this.selectedOperationIds.length) {
			// more inputs selected than available in this operation template
			return false;
		}
		const selectedTypes = this.selectedOperations.map(o => o.output.type);
		for (let i = 0; i < selectedTypes.length; i++) {
			if (selectedTypes[i] !== operation.inputTypes[i]) {
				// don't show this operation template if any of the selected type types does not match the input types of the operation template
				return false;
			}
		}
		return true;
	}

	onAddNode(operation: OperationTemplate): void {
		const request: AddOperationRequest = {
			pipelineId: this.pipelineId,
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
			pipelineId: this.pipelineId,
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
			pipelineId: this.pipelineId,
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
			this.$userFiles = this.filesService.getUserFileInfos();
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
}
