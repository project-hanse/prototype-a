import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {BaseHttpService} from '../../../core/_service/base-http.service';
import {ModelDto} from '../_model/model-dto';
import {ModelPredict} from '../_model/model-predict';
import {ModelTrainCompleteDto} from '../_model/model-train-complete-dto';

@Injectable({
	providedIn: 'root'
})
export class ModelService extends BaseHttpService {

	constructor(private httpClient: HttpClient) {
		super('api', httpClient);
	}

	public getModelDtos(): Observable<Array<ModelDto>> {
		return this.httpClient.get<Array<ModelDto>>(this.getLearningUrl('models'));
	}

	public trainModel(model: ModelDto): Observable<ModelTrainCompleteDto> {
		return this.httpClient.get<ModelTrainCompleteDto>(this.getLearningUrl('train/' + model.name));
	}

	public loadPrediction(model: ModelPredict, modelName?: string): Observable<string[]> {
		if (!modelName) {
			modelName = this.getSelectedModelLocally()?.name ?? 'model-3-complementnb';
		}
		return this.httpClient.post<string[]>(this.getLearningUrl(`predict/${modelName}`), model);
	}

	public storeSelectedModelLocally(value: ModelDto): void {
		localStorage.setItem('selectedModel', JSON.stringify(value));
	}

	public getSelectedModelLocally(): ModelDto | undefined {
		const model = localStorage.getItem('selectedModel');
		if (model) {
			return JSON.parse(model);
		}
		return undefined;
	}
}
