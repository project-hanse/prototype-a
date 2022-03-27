import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';

import {SettingsRoutingModule} from './settings-routing.module';
import {UserSettingsComponent} from './user-settings/user-settings.component';


@NgModule({
	declarations: [
		UserSettingsComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		SettingsRoutingModule
	]
})
export class SettingsModule {
}
