import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {CoreModule} from '../core/core.module';
import {UtilsModule} from '../utils/utils.module';
import {FileInfoListComponent} from './files-overview/file-info-list/file-info-list.component';
import {FilesOverviewComponent} from './files-overview/files-overview.component';

import {FilesRoutingModule} from './files-routing.module';


@NgModule({
	declarations: [
		FilesOverviewComponent,
  FileInfoListComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		FilesRoutingModule,
		UtilsModule
	]
})
export class FilesModule {
}
