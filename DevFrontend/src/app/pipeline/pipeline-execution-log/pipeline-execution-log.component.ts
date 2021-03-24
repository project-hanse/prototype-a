import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {MqttService} from 'ngx-mqtt';
import {Observable} from 'rxjs';
import {map, scan} from 'rxjs/operators';

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

  public executionEvents(pipelineId: string): Observable<any[]> {
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
            return acc.slice(-3);
          }, []),
        );
    }
    return this.$executionEvents;
  }

  ngOnDestroy(): void {
    this.mqttService.disconnect();
  }

}
