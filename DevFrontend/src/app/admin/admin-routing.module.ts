import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {PanelComponent} from './panel/panel.component';

const routes: Routes = [
	{
		path: 'panel',
		component: PanelComponent
	},
	{
		path: 'model',
		loadChildren: () => import('./model/model.module').then(m => m.ModelModule)
	},
	{
		path: 'pipeline-candidates',
		loadChildren: () => import('./pipeline-candidates/pipeline-candidates.module').then(m => m.PipelineCandidatesModule)
	},
	{
		path: '',
		redirectTo: 'panel',
		pathMatch: 'full'
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class AdminRoutingModule {
}
