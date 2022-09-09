import {Component, OnDestroy, OnInit} from '@angular/core';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Observable, Subscription} from 'rxjs';
import {environment} from '../../../../environments/environment';
import {ModelDto} from '../_model/model-dto';
import {LearningService} from '../_service/learning.service';
import {ModelService} from '../_service/model.service';

@Component({
	selector: 'ph-models-overview',
	templateUrl: './models-overview.component.html',
	styleUrls: ['./models-overview.component.scss']
})
export class ModelsOverviewComponent implements OnInit, OnDestroy {

	training: number = 0;

	private $models?: Observable<ModelDto[]>;

	private readonly subscriptions: Subscription = new Subscription();

	constructor(private modelsService: ModelService, private learningService: LearningService, private snackBar: MatSnackBar) {
	}

	ngOnInit(): void {
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	getModels(): Observable<Array<ModelDto>> {
		if (!this.$models) {
			this.$models = this.modelsService.getModelDtos();
		}
		return this.$models;
	}

	getMlflowUrl(model: ModelDto): string {
		return `${environment.mlflow}/#/models/${model.name}`;
	}

	test(): void {
		this.snackBar.open('Test\nho', 'Close', {
			duration: null,
			panelClass: ['snackbar-success']
		});
	}

	trainModel(model: ModelDto): void {
		this.training++;
		this.subscriptions.add(
			this.modelsService.trainModel(model).subscribe(
				result => {
					this.training--;
					this.$models = undefined;
					this.snackBar.open(
						`${model.name} trained \n train size: ${result.trainSize} \n accuracy: ${result.accuracy}\n cv accuracy: ${result.cvAccuracy}`,
						'Close',
						{
							duration: null,
							panelClass: ['snackbar-success']
						});
				},
				err => {
					this.snackBar.open(`Error while training model ${model.name}`, 'Close', {
						duration: 7500,
						panelClass: ['snackbar-error']
					});
					this.training--;
				})
		);
	}

	trainModelAllBackground(): void {
		this.training++;
		this.subscriptions.add(
			this.learningService.trainAllModelsBackground().subscribe(() => {
				this.snackBar.open('Training all models in background...', 'Close', {
					duration: 2500,
					panelClass: ['snackbar-success']
				});
				this.training--;
			})
		);
	}
}
