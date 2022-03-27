import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';

import {AdminRoutingModule} from './admin-routing.module';
import {PanelComponent} from './panel/panel.component';


@NgModule({
	declarations: [
		PanelComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		AdminRoutingModule,
	]
})
export class AdminModule {
}
