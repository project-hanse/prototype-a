import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {PipelineRoutingModule} from './pipeline-routing.module';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';
import {CoreModule} from '../core/core.module';


@NgModule({
  declarations: [
    PipelineListViewComponent,
    PipelineDetailViewComponent
  ],
    imports: [
        CommonModule,
        PipelineRoutingModule,
        CoreModule
    ]
})
export class PipelineModule {
}
