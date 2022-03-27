import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {UserSettingsComponent} from './user-settings/user-settings.component';

const routes: Routes = [
	{
		path: 'user',
		component: UserSettingsComponent
	},
	{
		path: '',
		redirectTo: 'user',
		pathMatch: 'full'
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class SettingsRoutingModule {
}
