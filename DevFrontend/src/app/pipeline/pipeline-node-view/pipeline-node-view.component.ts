import {Component, Input, OnInit} from '@angular/core';
import {Pipeline} from '../_model/pipeline';
import {Block} from '../_model/block';

@Component({
  selector: 'ph-pipeline-node-view',
  templateUrl: './pipeline-node-view.component.html',
  styleUrls: ['./pipeline-node-view.component.scss']
})
export class PipelineNodeViewComponent implements OnInit {

  @Input() pipeline: Pipeline;
  // @ts-ignore
  private network: vis.Network;

  constructor() {
  }

  ngOnInit(): void {
    this.network = this.renderGraph('mynetwork', this.pipeline);
  }

  // @ts-ignore
  public renderGraph(id: string, pipeline: Pipeline): vis.Network {
    const nodesArray = [];
    const edgesArray = [];

    this.buildArrays(nodesArray, edgesArray, pipeline.root);

    console.log(pipeline, nodesArray, edgesArray);

    // @ts-ignore
    const nodes = new vis.DataSet(nodesArray);

    // create an array with edges
    // @ts-ignore
    const edges = new vis.DataSet(edgesArray);

    // create a network
    const container = document.getElementById(id);
    const data = {
      nodes: nodes,
      edges: edges,
    };
    const options = {
      edges: {
        font: {
          align: 'top'
        },
        smooth: {
          type: 'dynamic',
          forceDirection: 'horizontal',
          roundness: 0.0
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
    // @ts-ignore
    return new vis.Network(container, data, options);
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
