import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {Pipeline} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';
import {Block} from '../_model/block';

@Component({
  selector: 'ph-pipeline-detail-view',
  templateUrl: './pipeline-detail-view.component.html',
  styleUrls: ['./pipeline-detail-view.component.scss']
})
export class PipelineDetailViewComponent implements OnInit {

  private $pipeline: Observable<Pipeline>;

  constructor(private route: ActivatedRoute, private pipelineService: PipelineService) {
  }

  ngOnInit(): void {
  }

  getId(): Observable<string> {
    return this.route.paramMap.pipe(map(p => p.get('id')));
  }

  getPipeline(id: string): Observable<Pipeline> {
    if (!this.$pipeline) {
      this.$pipeline = this.pipelineService.getPipeline(id);
    }
    return this.$pipeline;
  }

  renderGraph(id: string, pipeline: Pipeline): void {
    const nodesArray = [];
    const edgesArray = [];

    this.buildArrays(nodesArray, edgesArray, pipeline.root);

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
    var network = new vis.Network(container, data, options);

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
