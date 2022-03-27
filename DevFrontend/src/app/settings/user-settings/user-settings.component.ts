import {Component, OnDestroy, OnInit} from '@angular/core';
import {MatSelectChange} from '@angular/material/select';
import {combineLatest, Observable, Subscription} from 'rxjs';
import {map} from 'rxjs/operators';
import {ModelDto} from '../../admin/model/_model/model-dto';
import {ModelService} from '../../admin/model/_service/model.service';

@Component({
	selector: 'ph-user-settings',
	templateUrl: './user-settings.component.html',
	styleUrls: ['./user-settings.component.scss']
})
export class UserSettingsComponent implements OnInit, OnDestroy {
	$models: Observable<{ models: Array<ModelDto>, selection: ModelDto }>;

	private readonly subscriptions: Subscription = new Subscription();

	constructor(private modelsService: ModelService) {
	}

	ngOnInit(): void {
		this.$models = combineLatest([
			this.modelsService.getModelDtos()
		]).pipe(
			map(([models]) => {
				return {
					models,
					selection: this.modelsService.getSelectedModelLocally() ?? models[0]
				};
			})
		);
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	selectionChange($event: MatSelectChange): void {
		this.modelsService.storeSelectedModelLocally($event.value);
	}
}
