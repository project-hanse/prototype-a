import {HttpClient} from '@angular/common/http';
import {Component, OnInit} from '@angular/core';
import {Observable, timer} from 'rxjs';
import {switchMap} from 'rxjs/operators';
import {environment} from '../../../../environments/environment';

@Component({
	selector: 'ph-status-bar',
	templateUrl: './status-bar.component.html',
	styleUrls: ['./status-bar.component.scss']
})
export class StatusBarComponent implements OnInit {
	$apiHealthStatus: Observable<any>;

	constructor(private httpClient: HttpClient) {
	}

	ngOnInit(): void {
		this.$apiHealthStatus = timer(1, 5000).pipe(switchMap(() => this.httpClient.get(this.getApiUrl(), {responseType: 'text' as const})));
	}

	getIcon(status: string | undefined): string {
		if (status === 'Healthy') {
			return 'thumb_up_alt';
		}
		if (status === 'Degraded') {
			return 'pending';
		}
		return 'thumb_down_alt';
	}

	getApiUrl(): string {
		return `${environment.apiUrl}/health`;
	}
}
