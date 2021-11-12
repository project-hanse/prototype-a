import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';

import {DevToolsRoutingModule} from './dev-tools-routing.module';
import {StatusBarComponent} from './status/status-bar/status-bar.component';
import {StatusComponent} from './status/status.component';
import {UsernameComponent} from './username/username.component';


@NgModule({
	declarations: [
		StatusComponent,
		StatusBarComponent,
		UsernameComponent
	],
	exports: [
		StatusBarComponent,
		UsernameComponent
	],
	imports: [
		CoreModule,
		CommonModule,
		DevToolsRoutingModule,
	]
})
export class DevToolsModule {
}
