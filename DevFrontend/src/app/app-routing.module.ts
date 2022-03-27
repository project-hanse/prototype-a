import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';

const routes: Routes = [
	{
		path: 'pipeline',
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
		path: 'admin',
		loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
	},
	{
		path: 'settings',
		loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule)
	},
	{
		path: '',
		redirectTo: 'pipeline'
	},
	{
		path: '**',
		redirectTo: 'pipeline'
	}
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule]
})
export class AppRoutingModule {
}
