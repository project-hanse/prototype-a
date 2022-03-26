import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../../core/_service/base-http.service';
import {ModelDto} from '../_model/model-dto';
import {ModelTrainCompleteDto} from '../_model/model-train-complete-dto';

@Injectable({
	providedIn: 'root'
})
export class ModelService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('', httpClient);
	}

	public getModelDtos(): Observable<Array<ModelDto>> {
		return this.httpClient.get<Array<ModelDto>>(this.getLearningUrl('/models'));
	}

	public trainModel(model: ModelDto): Observable<ModelTrainCompleteDto> {
		return this.httpClient.get<ModelTrainCompleteDto>(this.getLearningUrl('/train/' + model.name));
	}
}
