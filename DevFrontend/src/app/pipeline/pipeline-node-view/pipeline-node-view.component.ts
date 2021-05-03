import {Component, Input, OnInit} from '@angular/core';
import {Pipeline} from '../_model/pipeline';
import {PipelineNode} from '../_model/pipelineNode';
import {Edge, Network, Node} from 'vis-network';
import {Options} from 'vis-network/declarations/network/Network';


@Component({
  selector: 'ph-pipeline-node-view',
  templateUrl: './pipeline-node-view.component.html',
  styleUrls: ['./pipeline-node-view.component.scss']
})
export class PipelineNodeViewComponent implements OnInit {

  @Input() pipeline: Pipeline;

  private network: Network;

  constructor() {
  }

  ngOnInit(): void {
    this.network = this.renderGraph('mynetwork', this.pipeline);
  }

  public renderGraph(id: string, pipeline: Pipeline): Network {
    const nodesArray = new Array<PipelineNode>();
    const edgesArray = new Array<Edge>();

    this.buildArrays(nodesArray, edgesArray, pipeline.root);

    // @ts-ignore
    const nodes = new vis.DataSet(nodesArray, {});

    // create an array with edges
    // @ts-ignore
    const edges = new vis.DataSet(edgesArray, {});

    // create a network
    const container = document.getElementById(id);
    const data = {
      nodes,
      edges,
    };
    const options: Options = {
      edges: {
        font: {
          align: 'top'
        },
        arrows: {
          to: {enabled: true, scaleFactor: 1, type: 'arrow'}
        }
      },
      layout: {
        hierarchical: {
          direction: 'LR',
          sortMethod: 'directed'
        }
      }
    };

    return new Network(container, data, options);
  }

  private buildArrays(nodesArray: any[], edgesArray: any[], blocks: PipelineNode[], parentId: string = null): void {
    blocks.forEach(block => {
      nodesArray.push({id: block.id, label: block.operation});
      if (parentId) {
        edgesArray.push({from: parentId, to: block.id});
      }
      this.buildArrays(nodesArray, edgesArray, block.successors, block.id);
    });
  }

}
