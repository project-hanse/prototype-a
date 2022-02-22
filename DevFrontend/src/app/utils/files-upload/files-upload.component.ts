import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {NgxFileDropEntry} from 'ngx-file-drop';
import {Observable, Subscription} from 'rxjs';
import {BaseResponse} from '../../core/_model/base-response';
import {FileInfoDto} from '../../files/_model/file-info-dto';
import {FilesService} from '../_services/files.service';

@Component({
	selector: 'ph-files-upload',
	templateUrl: './files-upload.component.html',
	styleUrls: ['./files-upload.component.scss']
})
export class FilesUploadComponent implements OnInit, OnDestroy {

	constructor(private filesService: FilesService) {
	}

	private readonly subscriptions = new Subscription();

	uploading: number = 0;
	files: NgxFileDropEntry[] = [];

	@Output()
	readonly fileUploaded: EventEmitter<FileInfoDto> = new EventEmitter<FileInfoDto>();

	@Output()
	readonly uploaded: EventEmitter<BaseResponse> = new EventEmitter<BaseResponse>();

	@Input()
	uploadText: string = 'Drop files here so they can be used in your pipelines.';

	@Input()
	public uploadFunction: (formData: FormData) => Observable<BaseResponse | FileInfoDto> = (formData: FormData) => {
		return this.filesService.uploadFile(formData);
	};

	ngOnInit(): void {
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	public dropped(files: NgxFileDropEntry[]): void {
		this.files = files;
		for (const droppedFile of files) {

			// Is it a file?
			if (droppedFile.fileEntry.isFile) {
				const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
				fileEntry.file((file: File) => {

					// Here you can access the real file
					// console.log(droppedFile.relativePath, file);
					this.uploading++;

					const formData = new FormData();
					if (file.lastModified) {
						formData.append('lastModified', new Date(file.lastModified).toISOString());
					}
					formData.append('file', file, droppedFile.relativePath);
					formData.append('fileName', file.name);

					this.subscriptions.add(this.sendToBackend(formData));
				});
			} else {
				// It was a directory (empty directories are added, otherwise only files)
				const fileEntry = droppedFile.fileEntry as FileSystemDirectoryEntry;
				console.log(droppedFile.relativePath, fileEntry);
			}
		}
	}

	private sendToBackend(formData: FormData): Subscription {
		return this.uploadFunction(formData).subscribe(
			next => {
				this.uploading--;
				const f = next as FileInfoDto;
				if (f) {
					this.fileUploaded.emit(f);
				}
				const r = next as BaseResponse;
				if (r) {
					this.uploaded.emit(r);
				}
			},
			error => {
				this.uploading--;
			}
		);
	}

	public fileOver(event): void {
		console.log(event);
	}

	public fileLeave(event): void {
		console.log(event);
	}

}
