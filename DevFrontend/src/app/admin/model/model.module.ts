import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';

import {ModelRoutingModule} from './model-routing.module';
import {OverviewComponent} from './overview/overview.component';


@NgModule({
	declarations: [
		OverviewComponent
	],
	imports: [
		CommonModule,
		ModelRoutingModule
	]
})
export class ModelModule {
}
