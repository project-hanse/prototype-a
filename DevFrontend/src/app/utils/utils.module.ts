import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {NgxFileDropModule} from 'ngx-file-drop';
import {CoreModule} from '../core/core.module';
import {FilesUploadComponent} from './files-upload/files-upload.component';
import { PipelineTitleComponent } from './pipeline-title/pipeline-title.component';


@NgModule({
	declarations: [
		FilesUploadComponent,
  PipelineTitleComponent
	],
	exports: [
		FilesUploadComponent,
		PipelineTitleComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		NgxFileDropModule
	]
})
export class UtilsModule {
}
