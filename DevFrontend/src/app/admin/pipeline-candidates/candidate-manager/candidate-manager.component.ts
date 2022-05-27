import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {catchError, merge, of, startWith} from 'rxjs';
import {map, switchMap} from 'rxjs/operators';
import {PipelineService} from '../../../pipeline/_service/pipeline.service';
import {PipelineCandidate} from '../_model/pipeline-candidate';

@Component({
	selector: 'ph-candidate-manager',
	templateUrl: './candidate-manager.component.html',
	styleUrls: ['./candidate-manager.component.scss']
})
export class CandidateManagerComponent implements OnInit, AfterViewInit {
	displayedColumns: Array<string> = ['completedAt', 'taskId', 'batchNumber', 'taskTypeId'];
	data: Array<PipelineCandidate> = [];

	resultsLength = 0;
	isLoadingResults = true;
	isRateLimitReached = false;

	@ViewChild(MatPaginator) paginator: MatPaginator;
	@ViewChild(MatSort) sort: MatSort;

	constructor(private pipelineService: PipelineService) {
	}

	ngOnInit(): void {
	}

	ngAfterViewInit(): void {
		// If the user changes the sort order, reset back to the first page.
		this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

		merge([this.sort.sortChange, this.paginator.page])
			.pipe(
				startWith({}),
				switchMap(() => {
					this.isLoadingResults = true;
					return this.pipelineService.getPipelineCandidates(
						this.sort.active,
						this.sort.direction,
						this.paginator.pageIndex,
					).pipe(catchError(() => of(null)));
				}),
				map(data => {
					// Flip flag to show that loading has finished.
					this.isLoadingResults = false;
					this.isRateLimitReached = data === null;

					if (data === null) {
						return [];
					}

					// Only refresh the result length if there is new data. In case of rate
					// limit errors, we do not want to reset the paginator to zero, as that
					// would prevent users from re-triggering requests.
					this.resultsLength = data.totalItems;
					return data.items;
				}),
			)
			.subscribe(data => (this.data = data));
	}
}
