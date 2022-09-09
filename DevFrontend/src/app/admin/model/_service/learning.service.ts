import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../../core/_service/base-http.service';

@Injectable({
	providedIn: 'root'
})
export class LearningService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('api/v1/learning', httpClient);
	}

	public trainAllModelsBackground(): Observable<void> {
		return this.httpClient.get<void>(this.getPipelinesUrl('train/all/background'));
	}
}
