import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';
import {UtilsModule} from '../utils/utils.module';
import {OperationConfigEditorComponent} from './operation-config-editor/operation-config-editor.component';
import {OperationResultPreviewComponent} from './operation-result-preview/operation-result-preview.component';
import {PipelineCreateComponent} from './pipeline-create/pipeline-create.component';
import {PipelineEditorComponent} from './pipeline-editor/pipeline-editor.component';
import {PipelineExecutionLogComponent} from './pipeline-execution-log/pipeline-execution-log.component';
import {PipelineGraphComponent} from './pipeline-graph-component/pipeline-graph.component';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';

import {PipelineRoutingModule} from './pipeline-routing.module';
import {PipelineToolboxComponent} from './pipeline-toolbox/pipeline-toolbox.component';
import {PipelineExportComponent} from './util-components/pipeline-export/pipeline-export.component';


@NgModule({
	declarations: [
		PipelineListViewComponent,
		PipelineEditorComponent,
		PipelineGraphComponent,
		PipelineExecutionLogComponent,
		PipelineToolboxComponent,
		OperationResultPreviewComponent,
		OperationConfigEditorComponent,
		PipelineCreateComponent,
		PipelineExportComponent
	],
	imports: [
		CommonModule,
		PipelineRoutingModule,
		CoreModule,
		UtilsModule
	]
})
export class PipelineModule {
}
