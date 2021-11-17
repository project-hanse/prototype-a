import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {FileInfoDto} from '../../files/_model/file-info-dto';

@Injectable({
	providedIn: 'root'
})
export class FilesService extends BaseHttpService {

	constructor(public httpClient: HttpClient) {
		super('api/v1/files', httpClient);
	}

	public getUserFileInfos(): Observable<Array<FileInfoDto>> {
		return this.httpClient.get<Array<FileInfoDto>>(this.getFilesUrl('info'));
	}

	public uploadFile(formData: FormData): Observable<FileInfoDto> {
		return this.httpClient.post<FileInfoDto>(this.getFilesUrl('upload'), formData);
	}
}
