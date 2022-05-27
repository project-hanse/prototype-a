import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {CandidateManagerComponent} from './candidate-manager/candidate-manager.component';

const routes: Routes = [
	{
		path: 'manager',
		component: CandidateManagerComponent,
	},
	{
		path: '',
		redirectTo: 'manager',
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class PipelineCandidatesRoutingModule {
}
