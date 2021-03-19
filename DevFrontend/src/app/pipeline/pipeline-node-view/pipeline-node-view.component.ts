import {Component, Input, OnInit} from '@angular/core';
import {Pipeline} from '../_model/pipeline';
import {Block} from '../_model/block';
import {DataSet, Edge, Network, Node} from 'vis-network';
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
    const nodesArray = new Array<Node>();
    const edgesArray = new Array<Edge>();

    this.buildArrays(nodesArray, edgesArray, pipeline.root);

    const nodes = new DataSet(nodesArray);

    // create an array with edges
    const edges = new DataSet(edgesArray);

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

  private buildArrays(nodesArray: any[], edgesArray: any[], blocks: Block[], parentId: string = null): void {
    blocks.forEach(block => {
      nodesArray.push({id: block.id, label: block.operation});
      if (parentId) {
        edgesArray.push({from: parentId, to: block.id});
      }
      this.buildArrays(nodesArray, edgesArray, block.successors, block.id);
    });
  }

}
