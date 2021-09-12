import {Component, Input, OnInit} from '@angular/core';
import {OperationsService} from '../_service/operations.service';
import {Observable} from 'rxjs';
import {OperationDto, OperationInputTypes} from '../_model/operation-dto';
import {map} from 'rxjs/operators';

@Component({
  selector: 'ph-pipeline-toolbox',
  templateUrl: './pipeline-toolbox.component.html',
  styleUrls: ['./pipeline-toolbox.component.scss']
})
export class PipelineToolboxComponent implements OnInit {

  @Input()
  public subtitle?: string;

  @Input()
  public selectedNodeIds: Array<string> = [];

  private $operationDtosGrouped?: Observable<Array<OperationDto>>;

  constructor(private operationsService: OperationsService) {
  }

  ngOnInit(): void {
  }

  getOperationDtos(): Observable<Array<OperationDto>> {
    if (!this.$operationDtosGrouped) {
      this.$operationDtosGrouped = this.operationsService.getOperations()
        .pipe(map(operations => operations.filter(operation => operation.operationInputType != OperationInputTypes.File)));
    }
    return this.$operationDtosGrouped;
  }

  isCompatibleWithSelection(operation: OperationDto): boolean {
    if (this.selectedNodeIds.length === 0) {
      return false;
    }
    if (operation.operationInputType === OperationInputTypes.Single && this.selectedNodeIds.length === 1) {
      return true;
    }
    if (operation.operationInputType === OperationInputTypes.Double && this.selectedNodeIds.length === 2) {
      return true;
    }
    return false;
  }
}
