import {Component, Input, OnInit} from '@angular/core';

@Component({
  selector: 'ph-pipeline-toolbox',
  templateUrl: './pipeline-toolbox.component.html',
  styleUrls: ['./pipeline-toolbox.component.scss']
})
export class PipelineToolboxComponent implements OnInit {

  @Input()
  public subtitle?: string;

  constructor() {
  }

  ngOnInit(): void {
  }

}
