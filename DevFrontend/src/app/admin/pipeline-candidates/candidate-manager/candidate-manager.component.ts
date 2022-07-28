import {SelectionModel} from '@angular/cdk/collections';
import {AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {catchError, combineLatest, of, ReplaySubject, startWith, Subject, Subscription} from 'rxjs';
import {map, switchMap} from 'rxjs/operators';
import {environment} from '../../../../environments/environment';
import {UsersService} from '../../../dev-tools/_services/users.service';
import {PipelineService} from '../../../pipeline/_service/pipeline.service';
import {PipelineCandidate} from '../_model/pipeline-candidate';

@Component({
	selector: 'ph-candidate-manager',
	templateUrl: './candidate-manager.component.html',
	styleUrls: ['./candidate-manager.component.scss']
})
export class CandidateManagerComponent implements OnInit, AfterViewInit, OnDestroy {
	displayedColumns: Array<string> = ['select', 'completedAt', 'taskId', 'batchNumber', 'rewardFunctionType', 'taskTypeId', 'actionsCount', 'simulationDuration'];
	readonly dataSource: MatTableDataSource<PipelineCandidate> = new MatTableDataSource([]);
	selection = new SelectionModel<PipelineCandidate>(true, []);
	private readonly reload: Subject<any> = new ReplaySubject<any>();

	resultsLength = 0;
	isLoadingResults = true;
	errorWhileLoading = false;
	loading = false;

	progressValue: number = 0;
	bufferValue: number = 0;
	processing: boolean = false;

	@ViewChild(MatPaginator) paginator: MatPaginator;

	@ViewChild(MatSort) sort: MatSort;
	private readonly subscriptions = new Subscription();

	constructor(private pipelineService: PipelineService, private usersService: UsersService, private matSnackBar: MatSnackBar) {
	}

	ngOnInit(): void {
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	ngAfterViewInit(): void {
		// If the user changes the sort order, reset back to the first page.
		this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);
		// select first sort column
		this.sort.active = 'completedAt';
		this.subscriptions.add(
			combineLatest([
				this.sort.sortChange.asObservable().pipe(startWith(null)),
				this.paginator.page.asObservable().pipe(startWith(null)),
				this.reload.pipe(startWith(null))])
				.pipe(
					switchMap(() => {
						this.isLoadingResults = true;
						console.log('switchMap', this.sort, this.paginator.page);
						return this.pipelineService.getPipelineCandidates({
								sort: this.sort.active,
								order: this.sort.direction,
								page: this.paginator.pageIndex,
								pageSize: this.paginator.pageSize
							}
						).pipe(catchError(() => of(null)));
					}),
					map(data => {
						// Flip flag to show that loading has finished.
						this.isLoadingResults = false;
						this.errorWhileLoading = data === null;

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
				.subscribe(data => (this.dataSource.data = data))
		);
	}

	/** Whether the number of selected elements matches the total number of rows. */
	isAllSelected(): boolean {
		const numSelected = this.selection.selected.length;
		const numRows = this.dataSource.data.length;
		return numSelected === numRows;
	}

	/** Selects all rows if they are not all selected; otherwise clear selection. */
	masterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
			return;
		}

		this.selection.select(...this.dataSource.data);
	}

	/** The label for the checkbox on the passed row */
	checkboxLabel(row?: PipelineCandidate): string {
		if (!row) {
			return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
		}
		// @ts-ignore
		return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.position + 1}`;
	}

	importCandidates(selected: PipelineCandidate[]): void {
		this.bufferValue = 0;
		this.progressValue = 0;
		this.loading = true;

		this.usersService.getCurrentUserInfo().subscribe(user => {
			this.importCandidatesForUser(selected, user.username);
		});
	}

	private importCandidatesForUser(selected: PipelineCandidate[], username: string): void {
		for (const pipelineCandidate of selected) {
			this.bufferValue += (1 / selected.length) * 100;
			this.subscriptions.add(
				this.pipelineService.importPipelineCandidate(pipelineCandidate.pipelineId, false, username).subscribe(
					(pipelineId) => {
						this.progressValue += (1 / selected.length) * 100;
						if (this.progressValue === 100) {
							this.matSnackBar.open('Pipelines imported successfully', 'Close', {
								duration: 5000,
							});
							this.loading = false;
						}
					},
					error => {
						this.matSnackBar.open(`Error while importing pipeline candidate ${pipelineCandidate.pipelineId}`, 'Close', {duration: 5000});
					})
			);
		}
	}

	getMetricsUrl(): string {
		return `${environment.pipelineApi}/api/v1/metrics/processing/candidates?sort=CreatedOn&order=desc&page=0&pageSize=100`;
	}

	processCandidates(numberOfCandidates: PipelineCandidate[]): void {
		this.processing = true;
		this.subscriptions.add(
			this.pipelineService.processCandidates(numberOfCandidates.map(c => c.pipelineId)).subscribe(
				processed => {
					this.matSnackBar.open(`${processed} candidates processed`, 'Close', {duration: 5000});
					this.processing = false;
					this.reload.next({});
					this.selection.clear();
				},
				error => {
					console.error(error);
					this.matSnackBar.open(`Error while processing candidates`, 'Close', {duration: 5000});
					this.processing = false;
				}
			)
		);
	}
}
