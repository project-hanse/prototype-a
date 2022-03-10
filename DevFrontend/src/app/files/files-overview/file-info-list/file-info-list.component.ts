import {Component, Input, OnInit} from '@angular/core';
import {FileInfoDto} from '../../_model/file-info-dto';

@Component({
	selector: 'ph-file-info-list',
	templateUrl: './file-info-list.component.html',
	styleUrls: ['./file-info-list.component.scss']
})
export class FileInfoListComponent implements OnInit {

	@Input() userFiles: Array<FileInfoDto>;

	@Input() toReadablyBytes: (size: number) => string;

	constructor() {
	}

	ngOnInit(): void {
	}

}
