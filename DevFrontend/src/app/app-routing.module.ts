import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';

const routes: Routes = [
	{
		path: 'pipelineId',
		loadChildren: () => import('./pipeline/pipeline.module').then(m => m.PipelineModule)
	},
	{
		path: 'dev',
		loadChildren: () => import('./dev-tools/dev-tools.module').then(m => m.DevToolsModule)
	},
	{
		path: 'files',
		loadChildren: () => import('./files/files.module').then(m => m.FilesModule)
	},
	{
		path: '',
		redirectTo: 'pipelineId'
	}
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule]
})
export class AppRoutingModule {
}
