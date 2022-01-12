import {HttpClient, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';
import {FileInfoDto} from '../../files/_model/file-info-dto';
import {Dataset} from '../../pipeline/_model/dataset';

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

	public getPlot(dataset: Dataset): Observable<string> {
		const params = new HttpParams({
			fromObject: {store: dataset.store, key: dataset.key}
		});
		return this.httpClient.get(this.getFilesUrl('plot'), {params, responseType: 'text'});
	}

	public getPlotUrl(dataset: Dataset): string {
		const url = new URL(this.getFilesUrl('plot'));
		url.searchParams.set('store', dataset.store);
		url.searchParams.set('key', dataset.key);
		return url.href;
	}
}
