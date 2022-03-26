import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ModelsOverviewComponent} from './models-overview/models-overview.component';

const routes: Routes = [
	{
		path: 'models-overview',
		component: ModelsOverviewComponent
	},
	{
		path: '',
		redirectTo: 'models-overview',
		pathMatch: 'full'
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class ModelRoutingModule {
}
