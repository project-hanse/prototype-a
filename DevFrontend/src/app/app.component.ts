import {Component} from '@angular/core';
import {environment} from '../environments/environment';

@Component({
	selector: 'ph-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.scss']
})
export class AppComponent {
	title = 'DevFrontend';

	isProd(): boolean {
		return environment.production;
	}

	getYear(): Date {
		return new Date();
	}

	getDatasetUrl(): string {
		return `${environment.datasetApi}/index.html`;
	}

	getMlflowUrl(): string {
		return `${environment.mlflow}`;
	}

	getHangfireUrl(): string {
		return `${environment.pipelineApi}/hangfire`;
	}

	getAdminerUrl(): string {
		return `${environment.adminer}?server=mysql`;
	}
}
