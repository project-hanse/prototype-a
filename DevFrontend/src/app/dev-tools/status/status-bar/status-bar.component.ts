import {Component, OnInit} from '@angular/core';
import {Observable, timer} from 'rxjs';
import {environment} from '../../../../environments/environment';
import {HttpClient} from '@angular/common/http';
import {switchMap} from 'rxjs/operators';

@Component({
  selector: 'ph-status-bar',
  templateUrl: './status-bar.component.html',
  styleUrls: ['./status-bar.component.scss']
})
export class StatusBarComponent implements OnInit {
  $s3Status: Observable<any>;

  constructor(private httpClient: HttpClient) {
  }

  ngOnInit(): void {
    this.$s3Status = timer(1, 3000).pipe(switchMap(() => this.httpClient.get(this.getS3Url())));
  }


  getIcon(status: any): string {
    if (!status?.services?.s3 || !status?.features?.persistence) {
      return 'thumb_down_alt';
    }
    if (status.services.s3 === 'running' && status.features.persistence !== 'initialized') {
      return 'pending';
    }
    if (status.services.s3 === 'running' && status.features.persistence === 'initialized') {
      return 'thumb_up_alt';
    }
    return 'help_outline';
  }

  getS3Url(): string {
    return `${environment.services.s3.url}/health`;
  }
}
