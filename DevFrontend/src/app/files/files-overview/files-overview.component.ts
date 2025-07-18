import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';
import {FilesService} from '../../utils/_services/files.service';
import {FileInfoDto} from '../_model/file-info-dto';

@Component({
	selector: 'ph-files-models-overview',
	templateUrl: './files-overview.component.html',
	styleUrls: ['./files-overview.component.scss']
})
export class FilesOverviewComponent implements OnInit {
	$userFiles?: Observable<Array<FileInfoDto>>;
	$defaultFiles?: Observable<Array<FileInfoDto>>;
	localProd: boolean = !environment.production;

	constructor(private filesService: FilesService) {
	}

	ngOnInit(): void {
		this.$userFiles = this.filesService.getUserFileInfos();
	}

	onFileUploaded($event: FileInfoDto): void {
		this.$userFiles = undefined;
	}

	getUserFiles(): Observable<Array<FileInfoDto>> {
		if (!this.$userFiles) {
			this.$userFiles = this.filesService.getUserFileInfos();
		}
		return this.$userFiles;
	}

	getDefaultFiles(): Observable<Array<FileInfoDto>> {
		if (!this.$defaultFiles) {
			this.$defaultFiles = this.filesService.getDefaultFileInfos();
		}
		return this.$defaultFiles;
	}

	toReadablyBytes(size: number): string {
		const val = Math.log2(size) / 10;
		return `${val.toFixed(2)} ${['Bytes', 'Kb', 'Mb', 'Gb', 'Tb'][Math.floor(val)]}`;
	}
}
