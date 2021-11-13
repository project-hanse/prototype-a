import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';

@Injectable({
	providedIn: 'root'
})
export class FilesService extends BaseHttpService {

	constructor(public httpClient: HttpClient) {
		super('api/v1/files', httpClient);
	}

	public uploadFiles(formData: FormData): Observable<any> {
		return this.httpClient.post(this.getFilesUrl('upload'), formData);
	}
}
