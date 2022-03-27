import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class BaseHttpService {

	constructor(private basePath: string, private http: HttpClient) {
	}

	protected getPipelinesUrl(...endpoint: string[]): string {
		return `${environment.pipelineApi}/${this.basePath}/${endpoint.join('/')}`;
	}

	protected getFilesUrl(...endpoint: string[]): string {
		return `${environment.filesApi}/${this.basePath}/${endpoint.join('/')}`;
	}

	protected getLearningUrl(...endpoint: string[]): string {
		return `${environment.learningApi}/${this.basePath}/${endpoint.join('/')}`;
	}

	public get<T>(...endpoint: string[]): Observable<T> {
		return this.http.get<T>(this.getPipelinesUrl(...endpoint));
	}
}
