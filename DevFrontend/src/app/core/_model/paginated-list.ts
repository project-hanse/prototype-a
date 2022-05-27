export interface PaginatedList<T> {
	totalItems: number;
	page: number;
	pageSize: number;
	items: T[];
}
