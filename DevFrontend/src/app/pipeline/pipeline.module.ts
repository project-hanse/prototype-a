import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {PipelineRoutingModule} from './pipeline-routing.module';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';
import {CoreModule} from '../core/core.module';
import { PipelineNodeViewComponent } from './pipeline-node-view/pipeline-node-view.component';


@NgModule({
  declarations: [
    PipelineListViewComponent,
    PipelineDetailViewComponent,
    PipelineNodeViewComponent
  ],
    imports: [
        CommonModule,
        PipelineRoutingModule,
        CoreModule
    ]
})
export class PipelineModule {
}
