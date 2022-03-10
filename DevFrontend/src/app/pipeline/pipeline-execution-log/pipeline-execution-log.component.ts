import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {MqttService} from 'ngx-mqtt';
import {Observable} from 'rxjs';
import {map, scan} from 'rxjs/operators';
import {environment} from '../../../environments/environment';
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
			hostname: environment.messageBrokerHost,
			path: environment.messageBrokerPath,
			port: environment.messageBrokerPort,
			protocol: environment.production ? 'wss' : 'ws',
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
						if (acc.length === 0) {
							acc.push(val);
							return acc;
						}
						if (acc[0].ExecutionId === val.ExecutionId) {
							acc.push(val);
							return acc;
						}
						return [val];
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

	private totalOperationsCount(last: FrontendExecutionNotification): number {
		return last.OperationsToBeExecuted + last.OperationsInExecution + last.OperationsFailedToExecute + last.OperationsExecuted;
	}

	public progressValue(last: FrontendExecutionNotification): number {
		return last.OperationsExecuted / this.totalOperationsCount(last) * 100;
	}

	public bufferValue(last: FrontendExecutionNotification): number {
		return last.OperationsInExecution / this.totalOperationsCount(last) * 100 + this.progressValue(last);
	}

	public sort(events: FrontendExecutionNotification[]): FrontendExecutionNotification[] {
		return events.sort((a, b) => {
			return new Date(a.CompletedAt).getTime() - new Date(b.CompletedAt).getTime();
		});
	}

	public getHtmlLinkToDataset(ResultDatasetKey: string): string {
		return `${environment.datasetApi}/api/dataframe/html/${ResultDatasetKey}`;
	}
}
