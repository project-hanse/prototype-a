import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {NodeService} from '../_service/node.service';

@Component({
  selector: 'ph-node-config-editor',
  templateUrl: './node-config-editor.component.html',
  styleUrls: ['./node-config-editor.component.scss']
})
export class NodeConfigEditorComponent implements OnInit, OnDestroy {
  @Input()
  pipelineId?: string;
  nodeIds?: Array<string>;
  $configs: Array<Observable<Map<string, string>>> = [];

  private readonly subscriptions: Subscription;

  constructor(private nodeService: NodeService) {
    this.subscriptions = new Subscription();
  }

  ngOnInit(): void {
  }

  getConfig(nodeId: string): Observable<Map<string, string>> {
    if (!this.$configs[nodeId]) {
      this.$configs[nodeId] = this.nodeService.getConfig(this.pipelineId, nodeId);
    }
    return this.$configs[nodeId];
  }

  onSubmit(nodeId: string, config: Map<string, string>): void {
    this.subscriptions.add(
      this.nodeService.updateConfig(this.pipelineId, nodeId, config).subscribe(
        res => console.log(res),
        err => console.error(err)
      )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
