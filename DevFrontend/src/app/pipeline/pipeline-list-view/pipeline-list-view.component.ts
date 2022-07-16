import {AfterViewInit, Component, EventEmitter, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {catchError, merge, Observable, startWith, Subscription} from 'rxjs';
import {map, switchMap} from 'rxjs/operators';
import {BaseResponse} from '../../core/_model/base-response';
import {PaginatedList} from '../../core/_model/paginated-list';
import {PipelineInfoDto} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-list-view',
	templateUrl: './pipeline-list-view.component.html',
	styleUrls: ['./pipeline-list-view.component.scss']
})
export class PipelineListViewComponent implements OnInit, AfterViewInit, OnDestroy {

	private $pipelines: Observable<PipelineInfoDto[]>;
	displayedColumns: string[] = ['name', 'userIdentifier', 'createdOn', 'changedOn'];
	data: PipelineInfoDto[] = [];
	resultsLength = 0;
	filterByUsernameLastValue?: string;
	private filterByUsername: EventEmitter<string | undefined> = new EventEmitter();

	isLoadingResults = true;
	@ViewChild(MatPaginator) paginator: MatPaginator;

	@ViewChild(MatSort) sort: MatSort;

	private readonly subscriptions: Subscription;
	uploadFunction = (formData: FormData) => {
		return this.pipelineService.importPipeline(formData);
	}

	constructor(private pipelineService: PipelineService) {
		this.subscriptions = new Subscription();
	}

	ngOnInit(): void {
	}

	ngAfterViewInit(): void {
		// If the user changes the sort order, reset back to the first page.
		this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));
		this.filterByUsername.subscribe(username => {this.filterByUsernameLastValue = username});

		merge(this.sort.sortChange, this.paginator.page, this.filterByUsername)
			.pipe(
				startWith({}),
				switchMap(() => {
					this.isLoadingResults = true;
					return this.pipelineService.getPipelineDtos({
							sort: this.sort.active,
							order: this.sort.direction,
							page: this.paginator.pageIndex,
							pageSize: this.paginator.pageSize
						},
						this.filterByUsernameLastValue
					).pipe(catchError((err) => {
						this.isLoadingResults = false;
						console.log(err);
						return [];
					}));
				}),
				map((data: PaginatedList<PipelineInfoDto>) => {
					// Flip flag to show that loading has finished.
					this.isLoadingResults = false;

					if (data === null) {
						return [];
					}
					this.resultsLength = data.totalItems;
					return data.items;
				}),
			)
			.subscribe(data => {
				this.data = data;
			});
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	onPipelineImported(response: BaseResponse): void {
		this.$pipelines = undefined;
	}

	userNameFilter(username?: string): void {
		console.log('userNameFilter: ' + username);
		this.filterByUsername.next(username);
	}
}
