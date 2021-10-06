import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {OperationsService} from '../_service/operations.service';
import {Observable, Subscription} from 'rxjs';
import {OperationDto, OperationDtoGroup, OperationInputTypes} from '../_model/operation-dto';
import {map} from 'rxjs/operators';
import {NodeService} from '../_service/node.service';
import {AddNodeRequest} from '../_model/add-node-request';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';
import {RemoveNodesRequest} from '../_model/remove-nodes-request';

@Component({
  selector: 'ph-pipeline-toolbox',
  templateUrl: './pipeline-toolbox.component.html',
  styleUrls: ['./pipeline-toolbox.component.scss']
})
export class PipelineToolboxComponent implements OnInit, OnDestroy {

  @Input()
  public subtitle?: string;

  @Input()
  public pipelineId: string;

  @Input()
  public selectedNodeIds: Array<string> = [];

  @Output()
  public readonly onPipelineChanged: EventEmitter<PipelineVisualizationDto>;

  private $operationDtosGroups?: Observable<Array<OperationDtoGroup>>;

  private readonly subscriptions: Subscription;

  constructor(private operationsService: OperationsService,
              private nodeService: NodeService) {
    this.subscriptions = new Subscription();
    this.onPipelineChanged = new EventEmitter<PipelineVisualizationDto>();
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
          this.onPipelineChanged.emit(response.pipelineVisualizationDto);
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
          this.onPipelineChanged.emit(response.pipelineVisualizationDto);
        },
        error => {
          console.error('Failed to add node', error);
        })
    );
  }
}
