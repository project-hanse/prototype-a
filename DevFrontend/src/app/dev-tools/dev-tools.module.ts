import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';

import {DevToolsRoutingModule} from './dev-tools-routing.module';
import {StatusBarComponent} from './status/status-bar/status-bar.component';
import {StatusComponent} from './status/status.component';


@NgModule({
	declarations: [
		StatusComponent,
		StatusBarComponent
	],
	exports: [
		StatusBarComponent
	],
	imports: [
		CommonModule,
		DevToolsRoutingModule,
		CoreModule
	]
})
export class DevToolsModule {
}
