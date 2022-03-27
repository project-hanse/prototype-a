import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../../core/core.module';

import {ModelRoutingModule} from './model-routing.module';
import {ModelsOverviewComponent} from './models-overview/models-overview.component';


@NgModule({
	declarations: [
		ModelsOverviewComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		ModelRoutingModule
	]
})
export class ModelModule {
}
