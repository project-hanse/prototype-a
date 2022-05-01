export interface Dataset {
	type: DatasetType;
	key: string;
	store: string;
}

/**
 * Used by pipeline editor to select output datasets for an operation.
 */
export interface DatasetView extends Dataset {
	selected?: boolean;
}

export enum DatasetType {
	File = 0,
	PdSeries = 1,
	PdDataFrame = 2,
	StaticPlot = 3,
	Prophet = 4,
	SklearnModel = 5
}
