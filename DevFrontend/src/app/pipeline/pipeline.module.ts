import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {PipelineRoutingModule} from './pipeline-routing.module';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';
import {CoreModule} from '../core/core.module';
import { PipelineGraphComponent } from './pipeline-graph-component/pipeline-graph.component';
import { PipelineExecutionLogComponent } from './pipeline-execution-log/pipeline-execution-log.component';
import { PipelineToolboxComponent } from './pipeline-toolbox/pipeline-toolbox.component';
import { NodeResultPreviewComponent } from './node-result-preview/node-result-preview.component';


@NgModule({
  declarations: [
    PipelineListViewComponent,
    PipelineDetailViewComponent,
    PipelineGraphComponent,
    PipelineExecutionLogComponent,
    PipelineToolboxComponent,
    NodeResultPreviewComponent
  ],
    imports: [
        CommonModule,
        PipelineRoutingModule,
        CoreModule
    ]
})
export class PipelineModule {
}
