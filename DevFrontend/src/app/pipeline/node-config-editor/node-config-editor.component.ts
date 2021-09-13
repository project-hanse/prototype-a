import {Component, Input, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {NodeService} from '../_service/node.service';

@Component({
  selector: 'ph-node-config-editor',
  templateUrl: './node-config-editor.component.html',
  styleUrls: ['./node-config-editor.component.scss']
})
export class NodeConfigEditorComponent implements OnInit {
  @Input()
  pipelineId?: string;
  nodeIds?: Array<string>;
  $configs: Array<Observable<any>> = [];

  constructor(private nodeService: NodeService) {
  }

  ngOnInit(): void {
  }

  getConfig(nodeId: string): Observable<any> {
    if (!this.$configs[nodeId]) {
      this.$configs[nodeId] = this.nodeService.getConfig(this.pipelineId, nodeId);
    }
    return this.$configs[nodeId];
  }

  stringify(config: any): string {
    return JSON.stringify(config);
  }
}
