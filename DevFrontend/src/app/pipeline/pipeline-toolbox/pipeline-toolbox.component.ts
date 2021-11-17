import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {OperationIds} from '../../core/_model/operation-ids';
import {FileInfoDto} from '../../files/_model/file-info-dto';
import {FilesService} from '../../utils/_services/files.service';
import {AddNodeRequest} from '../_model/add-node-request';
import {OperationDto, OperationDtoGroup, OperationInputTypes} from '../_model/operation-dto';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';
import {RemoveNodesRequest} from '../_model/remove-nodes-request';
import {NodeService} from '../_service/node.service';
import {OperationsService} from '../_service/operations.service';

@Component({
	selector: 'ph-pipeline-toolbox',
	templateUrl: './pipeline-toolbox.component.html',
	styleUrls: ['./pipeline-toolbox.component.scss']
})
export class PipelineToolboxComponent implements OnInit, OnDestroy {

	constructor(private operationsService: OperationsService, private nodeService: NodeService, private filesService: FilesService) {
		this.subscriptions = new Subscription();
		this.pipelineChanged = new EventEmitter<PipelineVisualizationDto>();
	}

	@Input()
	public subtitle?: string;

	@Input()
	public pipelineId: string;

	@Input()
	public selectedNodeIds: Array<string> = [];

	@Output()
	public readonly pipelineChanged: EventEmitter<PipelineVisualizationDto>;

	private $operationDtosGroups?: Observable<Array<OperationDtoGroup>>;
	private $userFiles?: Observable<Array<FileInfoDto>>;

	private readonly subscriptions: Subscription;
	operationsSearchText?: string;
	filesSearchText?: string;

	static isValidFileExtension(extension: string): boolean {
		return ['.csv', '.xlsx'].includes(extension);
	}

	private static getFileInputOperation(userFile: FileInfoDto): OperationDto {
		if (userFile.fileExtension === '.csv') {
			return {
				operationId: OperationIds.OpIdPdFileReadCsv,
				operationName: 'read_csv',
				operationFullName: 'Read CSV File',
				framework: 'pandas',
				description: '',
				operationInputType: OperationInputTypes.File,
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
			operationInputType: OperationInputTypes.File,
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

	getOperationDtos(): Observable<Array<OperationDtoGroup>> {
		if (!this.$operationDtosGroups) {
			this.$operationDtosGroups = this.operationsService.getOperationsGroups()
				.pipe(
					map(operationGroups => {
						for (const opGroup of operationGroups) {
							opGroup.operations = opGroup.operations.filter(operation => operation.operationInputType !== OperationInputTypes.File);
						}
						return operationGroups;
					}),
				);
		}
		return this.$operationDtosGroups;
	}

	showInAvailable(operation: OperationDto): boolean {
		if (this.operationsSearchText) {
			const searchIn = [operation.operationName, operation.operationFullName, operation.description, operation.sectionTitle];
			const searchInText = searchIn.join('').replace(' ', '').toLowerCase();
			if (!searchInText.includes(this.operationsSearchText.replace(' ', '').toLowerCase())) {
				return false;
			}
		}
		if (this.selectedNodeIds.length === 0) {
			return true;
		}
		if (operation.operationInputType === OperationInputTypes.Single && this.selectedNodeIds.length === 1) {
			return true;
		}
		if (operation.operationInputType === OperationInputTypes.Double && this.selectedNodeIds.length === 2) {
			return true;
		}
		return false;
	}

	onAddNode(operation: OperationDto): void {
		const request: AddNodeRequest = {
			pipelineId: this.pipelineId,
			operation,
			predecessorNodeIds: this.selectedNodeIds
		};
		this.subscriptions.add(
			this.nodeService.addNode(request).subscribe(
				response => {
					this.pipelineChanged.emit(response.pipelineVisualizationDto);
				},
				error => {
					console.error('Failed to add node', error);
				})
		);
	}

	onRemoveNodes(): void {
		const request: RemoveNodesRequest = {
			pipelineId: this.pipelineId,
			nodeIdsToBeRemoved: this.selectedNodeIds
		};
		this.subscriptions.add(
			this.nodeService.removeNodes(request).subscribe(
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
		const request: AddNodeRequest = {
			pipelineId: this.pipelineId,
			predecessorNodeIds: [],
			operation: PipelineToolboxComponent.getFileInputOperation(userFile),
			options: {
				objectBucket: userFile.bucketName,
				objectKey: userFile.objectKey
			}
		};
		this.subscriptions.add(
			this.nodeService.addNode(request).subscribe(
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
