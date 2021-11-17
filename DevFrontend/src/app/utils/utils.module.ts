import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {NgxFileDropModule} from 'ngx-file-drop';
import {CoreModule} from '../core/core.module';
import {FilesUploadComponent} from './files-upload/files-upload.component';


@NgModule({
	declarations: [
		FilesUploadComponent
	],
	exports: [
		FilesUploadComponent
	],
	imports: [
		CommonModule,
		CoreModule,
		NgxFileDropModule
	]
})
export class UtilsModule {
}
