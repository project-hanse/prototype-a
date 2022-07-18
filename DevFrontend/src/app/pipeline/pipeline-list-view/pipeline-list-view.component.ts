import {SelectionModel} from '@angular/cdk/collections';
import {AfterViewInit, Component, EventEmitter, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatSort} from '@angular/material/sort';
import {Router} from '@angular/router';
import {catchError, merge, Observable, startWith, Subscription} from 'rxjs';
import {map, switchMap} from 'rxjs/operators';
import {PipelineCandidate} from '../../admin/pipeline-candidates/_model/pipeline-candidate';
import {BaseResponse} from '../../core/_model/base-response';
import {PaginatedList} from '../../core/_model/paginated-list';
import {UsersService} from '../../dev-tools/_services/users.service';
import {PipelineInfoDto} from '../_model/pipeline';
import {PipelineService} from '../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-list-view',
	templateUrl: './pipeline-list-view.component.html',
	styleUrls: ['./pipeline-list-view.component.scss']
})
export class PipelineListViewComponent implements OnInit, AfterViewInit, OnDestroy {

	private $pipelines: Observable<PipelineInfoDto[]>;
	displayedColumns: string[] = ['select', 'name', 'userIdentifier', 'createdOn'];
	data: PipelineInfoDto[] = [];
	selection = new SelectionModel<PipelineInfoDto>(true, []);
	resultsLength = 0;
	filterByUsernameLastValue?: string;
	private readonly filterByUsername: EventEmitter<string | undefined> = new EventEmitter();
	private readonly reload: EventEmitter<void> = new EventEmitter();

	isLoadingResults = true;
	@ViewChild(MatPaginator) paginator: MatPaginator;

	@ViewChild(MatSort) sort: MatSort;

	private readonly subscriptions: Subscription;
	uploadFunction = (formData: FormData) => {
		return this.pipelineService.importPipeline(formData);
	};

	constructor(
		private pipelineService: PipelineService,
		private router: Router,
		private snackBar: MatSnackBar,
		private userService: UsersService) {
		this.subscriptions = new Subscription();
	}

	ngOnInit(): void {
		if (window.innerWidth > 666) {
			if (window.innerWidth > 768) {
				this.displayedColumns.push('changedOn');
			}
			// add 'changedOn' to end of displayedColumns
			this.displayedColumns.push('lastRun');
		}
		this.subscriptions.add(
			this.userService.getCurrentUserInfo().subscribe(user => {
				if (user) {
					this.userNameFilter(user.username);
				}
			})
		);
	}

	ngAfterViewInit(): void {
		// If the user changes the sort order, reset back to the first page.
		this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));
		this.filterByUsername.subscribe(username => {
			this.filterByUsernameLastValue = username;
		});

		merge(this.sort.sortChange, this.paginator.page, this.filterByUsername, this.reload)
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

	/** Whether the number of selected elements matches the total number of rows. */
	isAllSelected(): boolean {
		const numSelected = this.selection.selected.length;
		const numRows = this.data.length;
		return numSelected === numRows;
	}

	/** Selects all rows if they are not all selected; otherwise clear selection. */
	masterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
			return;
		}

		this.selection.select(...this.data);
	}

	/** The label for the checkbox on the passed row */
	checkboxLabel(row?: PipelineCandidate): string {
		if (!row) {
			return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
		}
		// @ts-ignore
		return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.position + 1}`;
	}

	userNameFilter(username?: string): void {
		this.filterByUsername.next(username);
	}

	createNewPipeline(): void {
		this.subscriptions.add(
			this.pipelineService.createFromTemplate({}).subscribe(response => {
					this.snackBar.open('New pipeline created', '', {duration: 2000});
					this.router.navigate(['/pipeline', response.pipelineId]);
				}
			)
		);
	}

	enqueuePipelines(selected: PipelineInfoDto[]): void {
		this.isLoadingResults = true;
		this.subscriptions.add(
			this.pipelineService.enqueuePipelines(selected.map(p => p.id)).subscribe(response => {
					this.snackBar.open(response.length + ' pipeline(s) enqueued', '', {duration: 2000});
					this.isLoadingResults = false;
					this.reload.emit();
					// dirty polling to check if execution is complete
					// call this.reload.emit() every 1.5 seconds for 10 times
					let counter = 0;
					const interval = setInterval(() => {
						this.reload.emit();
						counter++;
						if (counter === 10) {
							clearInterval(interval);
						}
					}, 1500);
				}
			)
		);
	}

	deletePipelines(pipelines: PipelineInfoDto[]): void {
		if (confirm('Are you sure you want to delete the selected pipeline(s)?')) {
			this.isLoadingResults = true;
			this.subscriptions.add(
				this.pipelineService.deletePipelines(pipelines.map(p => p.id)).subscribe(response => {
						this.snackBar.open(response + ' pipeline(s) deleted', '', {duration: 2000});
						this.reload.next();
						this.selection.clear();
						this.isLoadingResults = false;
					}
				)
			);
		}
	}
}
