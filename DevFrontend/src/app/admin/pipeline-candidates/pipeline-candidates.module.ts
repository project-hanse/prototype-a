import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../../core/core.module';
import {CandidateManagerComponent} from './candidate-manager/candidate-manager.component';

import {PipelineCandidatesRoutingModule} from './pipeline-candidates-routing.module';


@NgModule({
	declarations: [
		CandidateManagerComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		PipelineCandidatesRoutingModule
	]
})
export class PipelineCandidatesModule {
}
