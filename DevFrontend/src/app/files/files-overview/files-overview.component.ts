import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {FilesService} from '../../utils/_services/files.service';
import {FileInfoDto} from '../_model/file-info-dto';

@Component({
	selector: 'ph-files-overview',
	templateUrl: './files-overview.component.html',
	styleUrls: ['./files-overview.component.scss']
})
export class FilesOverviewComponent implements OnInit {
	$userFiles?: Observable<Array<FileInfoDto>>;

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

	toReadablyBytes(size: number): string {
		const val = Math.log2(size) / 10;
		return `${val.toFixed(2)} ${['Bytes', 'Kb', 'Mb', 'Gb', 'Tb'][Math.floor(val)]}`;
	}
}
