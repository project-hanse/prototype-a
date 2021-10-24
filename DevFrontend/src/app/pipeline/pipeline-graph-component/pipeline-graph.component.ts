import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {Subscription} from 'rxjs';
import {Data, DataSet, Edge, Network, Node, Options} from 'vis';
import {PipelineVisualizationDto} from '../_model/pipeline-visualization.dto';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-node-view',
	templateUrl: './pipeline-graph.component.html',
	styleUrls: ['./pipeline-graph.component.scss']
})
export class PipelineGraphComponent implements OnInit {

	@Input() pipelineId: string;

	@Output()
	readonly selectedNodeIdsChange: EventEmitter<Array<string>>;

	readonly networkElementId = 'network';
	private network?: Network;

	private readonly subscriptions: Subscription;

	constructor(private pipelineService: PipelineService) {
		this.subscriptions = new Subscription();
		this.selectedNodeIdsChange = new EventEmitter<Array<string>>();
	}

	ngOnInit(): void {
		this.subscriptions.add(
			this.pipelineService.getPipelineForVisualization(this.pipelineId).subscribe(
				res => {
					this.displayPipeline(res);
				},
				error => {
					console.error('Failed to load pipeline', error);
				}
			)
		);
	}

	public displayPipeline(pipeline: PipelineVisualizationDto): void {
		this.network = this.renderGraph(pipeline);
		this.network.on('click', (properties) => {
			const ids = properties.nodes;
			this.selectedNodeIdsChange.emit(ids);
		});
	}

	private renderGraph(pipeline: PipelineVisualizationDto): Network {
		const nodes = new DataSet<Node>(pipeline.nodes, {});
		const edges = new DataSet<Edge>(pipeline.edges, {});

		const container = document.getElementById(this.networkElementId);
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

		if (this.network) {
			this.network.setData(data);
		} else {
			this.network = new Network(container, data, options);
		}

		return this.network;
	}

}
