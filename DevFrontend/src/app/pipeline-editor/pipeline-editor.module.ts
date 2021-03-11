import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {PipelineEditorRoutingModule} from './pipeline-editor-routing.module';
import {ViewPipelineComponent} from './view-pipeline/view-pipeline.component';


@NgModule({
  declarations: [
    ViewPipelineComponent
  ],
  imports: [
    CommonModule,
    PipelineEditorRoutingModule
  ]
})
export class PipelineEditorModule {
}
