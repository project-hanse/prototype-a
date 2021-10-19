import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';

const routes: Routes = [
	{
		path: ':id',
		component: PipelineDetailViewComponent
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
