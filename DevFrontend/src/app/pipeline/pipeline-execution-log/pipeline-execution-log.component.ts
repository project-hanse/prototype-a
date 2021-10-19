import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {MqttService} from 'ngx-mqtt';
import {Observable} from 'rxjs';
import {map, scan} from 'rxjs/operators';
import {FrontendExecutionNotification} from '../_model/frontend-execution-notification';

@Component({
	selector: 'ph-pipeline-execution-log',
	templateUrl: './pipeline-execution-log.component.html',
	styleUrls: ['./pipeline-execution-log.component.scss']
})
export class PipelineExecutionLogComponent implements OnInit, OnDestroy {

	@Input() pipelineId: string;
	private readonly mqttService: MqttService;
	private $executionEvents: Observable<any>;

	constructor() {
		this.mqttService = new MqttService({
			connectOnCreate: false,
			hostname: 'localhost',
			port: 9002,
			protocol: 'ws',
			clientId: 'some-dev-frontend'
		});
		this.mqttService.onConnect.subscribe(e => {
			console.log('Connecting ', e);
		});
		this.mqttService.onClose.subscribe(e => {
			console.log('Closing ', e);
		});
	}

	ngOnInit(): void {
		this.mqttService.connect();
	}

	public executionEvents(pipelineId: string): Observable<FrontendExecutionNotification[]> {
		if (!this.$executionEvents) {
			this.$executionEvents = this.mqttService
				.observe('pipeline/event/' + pipelineId)
				.pipe(
					map(m => {
						const stringBuf = m.payload.toString();
						const obj = JSON.parse(stringBuf);
						console.log(obj);
						return obj;
					}),
					scan((acc, val) => {
						acc.push(val);
						// TODO display number of nodes in pipeline
						return acc.slice(-5);
					}, [])
				);
		}
		return this.$executionEvents;
	}

	ngOnDestroy(): void {
		this.mqttService.disconnect();
	}

	public getLast(events: FrontendExecutionNotification[]): FrontendExecutionNotification | null {
		if (!events || events.length === 0) {
			return null;
		}
		return events[events.length - 1];
	}

	private totalNodes(last: FrontendExecutionNotification): number {
		return last.NodesToBeExecuted + last.NodesInExecution + last.NodesFailedToExecute + last.NodesExecuted;
	}

	public progressValue(last: FrontendExecutionNotification): number {
		return last.NodesExecuted / this.totalNodes(last) * 100;
	}

	public bufferValue(last: FrontendExecutionNotification): number {
		return last.NodesInExecution / this.totalNodes(last) * 100 + this.progressValue(last);
	}

	public sort(events: FrontendExecutionNotification[]): FrontendExecutionNotification[] {
		return events.sort((a, b) => {
			return new Date(a.CompletedAt).getTime() - new Date(b.CompletedAt).getTime();
		});
	}
}
