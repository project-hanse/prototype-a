import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {Pipeline} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

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
}
