import {Component, Input, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {NodeService} from '../_service/node.service';

@Component({
  selector: 'ph-node-result-preview',
  templateUrl: './node-result-preview.component.html',
  styleUrls: ['./node-result-preview.component.scss']
})
export class NodeResultPreviewComponent implements OnInit {

  @Input()
  pipelineId?: string;
  nodeIds?: Array<string>;

  private $hashes = {};
  private $previewHtml = {};

  constructor(private nodeService: NodeService) {
  }

  ngOnInit(): void {
  }

  getHash(nodeId: string): Observable<{ hash: string }> {
    if (!this.$hashes[nodeId]) {
      this.$hashes[nodeId] = this.nodeService.getResultHash(this.pipelineId, nodeId);
    }
    return this.$hashes[nodeId];
  }

  getPreviewHtml(hash: string): Observable<string> {
    if (!this.$previewHtml[hash]) {
      this.$previewHtml[hash] = this.nodeService.getPreviewHtml(hash);
    }
    return this.$previewHtml[hash];
  }
}
