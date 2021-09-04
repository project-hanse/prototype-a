import {Component, Input, OnInit} from '@angular/core';
import {Network} from 'vis-network';
import {Options} from 'vis-network/declarations/network/Network';
import {PipelineService} from '../_service/pipeline.service';
import {Subscription} from 'rxjs';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';


@Component({
  selector: 'ph-pipeline-node-view',
  templateUrl: './pipeline-node-view.component.html',
  styleUrls: ['./pipeline-node-view.component.scss']
})
export class PipelineNodeViewComponent implements OnInit {

  @Input() pipelineId: string;

  private network?: Network;

  private readonly subscriptions: Subscription;

  constructor(private pipelineService: PipelineService) {
    this.subscriptions = new Subscription();
  }

  ngOnInit(): void {
    this.subscriptions.add(
      this.pipelineService.getPipelineForVisualization(this.pipelineId).subscribe(
        res => {
          this.network = this.renderGraph('mynetwork', res);
          this.network.on('click', function(properties) {
            const ids = properties.nodes;
            console.log('clicked node id:', ids);
          });
        },
        error => {
          console.error('Failed to load pipeline', error);
        }
      )
    );

  }

  public renderGraph(id: string, pipeline: PipelineVisualizationDto): Network {

    // @ts-ignore
    const nodes = new vis.DataSet(pipeline.nodes, {});

    // create an array with edges
    // @ts-ignore
    const edges = new vis.DataSet(pipeline.edges, {});

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

}
