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

	protected getUrl(...endpoint: string[]): string {
		return `${environment.apiUrl}/${this.basePath}/${endpoint.join('/')}`;
	}

	public get<T>(...endpoint: string[]): Observable<T> {
		return this.http.get<T>(this.getUrl(...endpoint));
	}
}
