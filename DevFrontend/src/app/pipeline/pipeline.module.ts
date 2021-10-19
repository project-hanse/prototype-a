import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';
import {NodeConfigEditorComponent} from './node-config-editor/node-config-editor.component';
import {NodeResultPreviewComponent} from './node-result-preview/node-result-preview.component';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';
import {PipelineExecutionLogComponent} from './pipeline-execution-log/pipeline-execution-log.component';
import {PipelineGraphComponent} from './pipeline-graph-component/pipeline-graph.component';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';

import {PipelineRoutingModule} from './pipeline-routing.module';
import {PipelineToolboxComponent} from './pipeline-toolbox/pipeline-toolbox.component';


@NgModule({
	declarations: [
		PipelineListViewComponent,
		PipelineDetailViewComponent,
		PipelineGraphComponent,
		PipelineExecutionLogComponent,
		PipelineToolboxComponent,
		NodeResultPreviewComponent,
		NodeConfigEditorComponent
	],
	imports: [
		CommonModule,
		PipelineRoutingModule,
		CoreModule
	]
})
export class PipelineModule {
}
