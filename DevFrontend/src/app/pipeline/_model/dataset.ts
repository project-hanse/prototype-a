export interface Dataset {
	type: DatasetType;
	key: string;
	store: string;
}

export enum DatasetType {
	File = 0,
	PdSeries = 1,
	PdDataFrame = 2,
	StaticPlot = 3,
}
