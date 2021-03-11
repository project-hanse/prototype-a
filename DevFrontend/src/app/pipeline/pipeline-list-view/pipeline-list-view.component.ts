import {Component, OnInit} from '@angular/core';
import {PipelineService} from '../_service/pipeline.service';
import {Observable} from 'rxjs';
import {Pipeline} from '../_model/pipeline';

@Component({
  selector: 'ph-pipeline-list-view',
  templateUrl: './pipeline-list-view.component.html',
  styleUrls: ['./pipeline-list-view.component.scss']
})
export class PipelineListViewComponent implements OnInit {

  private $pipelines: Observable<Pipeline[]>;

  constructor(private pipelineService: PipelineService) {
  }

  ngOnInit(): void {
  }

  public getPipelines(): Observable<Pipeline[]> {
    if (!this.$pipelines) {
      this.$pipelines = this.pipelineService.getPipelines();
    }
    return this.$pipelines;
  }

}
