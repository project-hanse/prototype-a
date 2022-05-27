import {SortDirection} from '@angular/material/sort';

export interface Pagination {
	sort: string;
	order: SortDirection;
	page: number;
	pageSize: number;
}
