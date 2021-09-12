import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {PipelineService} from '../_service/pipeline.service';
import {Subscription} from 'rxjs';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';
import {Network, DataSet, Options, Data, Node, Edge} from 'vis';

@Component({
  selector: 'ph-pipeline-node-view',
  templateUrl: './pipeline-graph.component.html',
  styleUrls: ['./pipeline-graph.component.scss']
})
export class PipelineGraphComponent implements OnInit {

  @Input() pipelineId: string;

  @Output() readonly onSelectedNodeIdsChange: EventEmitter<Array<string>>;

  private network?: Network;

  private readonly subscriptions: Subscription;

  constructor(private pipelineService: PipelineService) {
    this.subscriptions = new Subscription();
    this.onSelectedNodeIdsChange = new EventEmitter<Array<string>>();
  }

  ngOnInit(): void {
    this.subscriptions.add(
      this.pipelineService.getPipelineForVisualization(this.pipelineId).subscribe(
        res => {
          this.network = this.renderGraph('network', res);
          this.network.on('click', (properties) => {
            const ids = properties.nodes;
            this.onSelectedNodeIdsChange.emit(ids);
          });
        },
        error => {
          console.error('Failed to load pipeline', error);
        }
      )
    );
  }

  public renderGraph(id: string, pipeline: PipelineVisualizationDto): Network {
    const nodes = new DataSet<Node>(pipeline.nodes, {});
    const edges = new DataSet<Edge>(pipeline.edges, {});

    const container = document.getElementById(id);
    const data: Data = {
      nodes,
      edges
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
      },
      interaction: {
        multiselect: true
      }
    };

    return new Network(container, data, options);
  }

}
