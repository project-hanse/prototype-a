import {Component, OnDestroy, OnInit} from '@angular/core';
import {NgxFileDropEntry} from 'ngx-file-drop';
import {Subscription} from 'rxjs';
import {FilesService} from '../_services/files.service';

@Component({
	selector: 'ph-files-upload',
	templateUrl: './files-upload.component.html',
	styleUrls: ['./files-upload.component.scss']
})
export class FilesUploadComponent implements OnInit, OnDestroy {
	uploading: number = 0;
	files: NgxFileDropEntry[] = [];

	private readonly subscriptions = new Subscription();

	constructor(private filesService: FilesService) {
	}

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

					this.subscriptions.add(
						this.filesService.uploadFiles(formData).subscribe(
							next => {
								this.uploading--;
								console.log(next);
							},
							error => {
								this.uploading--;
								console.log(error);
							}
						));
				});
			} else {
				// It was a directory (empty directories are added, otherwise only files)
				const fileEntry = droppedFile.fileEntry as FileSystemDirectoryEntry;
				console.log(droppedFile.relativePath, fileEntry);
			}
		}
	}

	public fileOver(event): void {
		console.log(event);
	}

	public fileLeave(event): void {
		console.log(event);
	}

}
