import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {FilesOverviewComponent} from './files-overview/files-overview.component';

const routes: Routes = [
	{
		path: 'overview',
		component: FilesOverviewComponent
	}, {
		path: '**',
		redirectTo: 'overview'
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class FilesRoutingModule {
}
