import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {PipelineEditorComponent} from './pipeline-editor/pipeline-editor.component';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';

const routes: Routes = [
	{
		path: ':id',
		component: PipelineEditorComponent
	},
	{
		path: '',
		component: PipelineListViewComponent
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class PipelineRoutingModule {
}
