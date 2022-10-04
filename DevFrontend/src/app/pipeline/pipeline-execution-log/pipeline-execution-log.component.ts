import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {MqttClient} from 'mqtt';
import * as mqtt from 'mqtt/dist/mqtt.min';
import {Observable, scan, Subject} from 'rxjs';
import * as uuid from 'uuid';
import {environment} from '../../../environments/environment';
import {FilesService} from '../../utils/_services/files.service';
import {Dataset, DatasetType} from '../_model/dataset';
import {FrontendExecutionNotification} from '../_model/frontend-execution-notification';
import {PipelineInfoDto} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-execution-log',
	templateUrl: './pipeline-execution-log.component.html',
	styleUrls: ['./pipeline-execution-log.component.scss']
})
export class PipelineExecutionLogComponent implements OnInit, OnDestroy {
	constructor(private filesService: FilesService, private pipelineService: PipelineService) {
	}

	private static executedTopicPrefix: string = 'pipeline/event/';

	@Input() pipelineId: string;
	private client?: MqttClient;
	private readonly pipelineNotificationSubjects = new Map<string, Subject<FrontendExecutionNotification>>();
	$eventStream?: Observable<FrontendExecutionNotification[]>;
	private $pipelineInfoDto?: Observable<PipelineInfoDto>;

	ngOnInit(): void {
		this.setupMqttClient();
		if (this.pipelineId) {
			this.$eventStream = this.getEventsForLastExecution(this.subscribeToPipelineEvents(this.pipelineId));
		}
	}

	ngOnDestroy(): void {
		for (const key of this.pipelineNotificationSubjects.keys()) {
			this.client?.unsubscribe(`${PipelineExecutionLogComponent.executedTopicPrefix}${key}`);
		}
		this.client?.end(true);
	}

	private setupMqttClient(): void {
		this.client = mqtt.connect(null, {
			clientId: 'frontend-' + uuid.v4(),
			path: environment.messageBrokerPath,
			servers: [
				{
					host: environment.messageBrokerHost,
					port: environment.messageBrokerPort,
					protocol: 'wss',
				},
				{
					host: environment.messageBrokerHost,
					port: environment.messageBrokerPort,
					protocol: 'ws',
				},
				{
					host: environment.messageBrokerHost,
					port: environment.messageBrokerPortAlternative,
					protocol: 'wss',
				},
				{
					host: environment.messageBrokerHost,
					port: environment.messageBrokerPortAlternative,
					protocol: 'ws',
				}
			]
		});
		this.client.on('connect', (c) => {
			console.log('connected', c);
		});
		this.client.on('disconnect', (c) => {
			console.log('disconnected', c);
		});
		this.client.on('error', (e) => {
			console.error('error', e);
		});
		this.client.on('message', (topic, buffer) => {
			try {
				const message = JSON.parse(buffer.toString());
				const pipelineId = topic.replace(PipelineExecutionLogComponent.executedTopicPrefix, '');
				if (this.pipelineNotificationSubjects.has(pipelineId)) {
					this.pipelineNotificationSubjects.get(pipelineId).next(message);
				}
			} catch (e) {
				console.error(`Failed to decode message from topic '${topic}'`, e);
			}
		});
	}

	private subscribeToPipelineEvents(pipelineId: string): Observable<FrontendExecutionNotification> {
		if (this.pipelineNotificationSubjects.has(pipelineId)) {
			return this.pipelineNotificationSubjects.get(pipelineId);
		} else {
			this.client.subscribe(`${PipelineExecutionLogComponent.executedTopicPrefix}${pipelineId}`, {qos: 1}, (err, granted) => {
				console.log('subscribed', err, granted);
			});
			const subject = new Subject<FrontendExecutionNotification>();
			this.pipelineNotificationSubjects.set(pipelineId, subject);
			return subject;
		}
	}

	private getEventsForLastExecution(stream: Observable<FrontendExecutionNotification>): Observable<FrontendExecutionNotification[]> {
		return stream.pipe(
			scan((acc, val) => {
				if (acc.length === 0) {
					acc.push(val);
					return acc;
				}
				// @ts-ignore
				if (acc[0].ExecutionId === val.ExecutionId) {
					acc.push(val);
					return acc;
				}
				return [val];
			}, []));
	}

	public getPipelineInfoDto(pipelineId: string): Observable<PipelineInfoDto> {
		if (!this.$pipelineInfoDto) {
			this.$pipelineInfoDto = this.pipelineService.getPipelineDto(pipelineId);
		}
		return this.$pipelineInfoDto;
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

	public getHtmlLinkToDataset(dataset: Dataset): string {
		// needs to be fixed in serialization of MQTT messages
		// @ts-ignore
		if (dataset['Type'] === DatasetType.PdDataFrame) {
			return `${environment.datasetApi}/api/dataframe/key/${dataset['Key']}?format=html`;
		}
		if (dataset['Type'] === DatasetType.PdSeries) {
			return `${environment.datasetApi}/api/series/key/${dataset['Key']}?format=html`;
		} else if (dataset['Type'] === DatasetType.StaticPlot) {
			return this.filesService.getPlotUrl({
				type: dataset['Type'],
				key: dataset['Key'],
				store: dataset['Store']
			});
		}
		return '#';
	}
}
