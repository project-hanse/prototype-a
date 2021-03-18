import {Component, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {Pipeline} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
  selector: 'ph-pipeline-detail-view',
  templateUrl: './pipeline-detail-view.component.html',
  styleUrls: ['./pipeline-detail-view.component.scss']
})
export class PipelineDetailViewComponent implements OnInit, OnDestroy {

  private readonly subscriptions: Subscription;

  private $pipeline: Observable<Pipeline>;

  constructor(private route: ActivatedRoute, private pipelineService: PipelineService) {
    this.subscriptions = new Subscription();
  }

  ngOnInit(): void {
  }

  public getId(): Observable<string> {
    return this.route.paramMap.pipe(map(p => p.get('id')));
  }

  public getPipeline(id: string): Observable<Pipeline> {
    if (!this.$pipeline) {
      this.$pipeline = this.pipelineService.getPipeline(id);
    }
    return this.$pipeline;
  }

  public executePipeline(id: string): void {
    this.subscriptions.add(
      this.pipelineService.executePipeline(id)
        .subscribe(
          executionId => console.log('Started execution' + executionId)
        )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
