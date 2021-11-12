import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../core/_service/base-http.service';

@Injectable({
	providedIn: 'root'
})
export class UsersService extends BaseHttpService {

	constructor(public httpClient: HttpClient) {
		super('api/v1/users', httpClient);
	}

	public getCurrentUserInfo(): Observable<{ username: string }> {
		return this.httpClient.get<{ username: string }>(this.getUrl('current', 'info'));
	}
}
