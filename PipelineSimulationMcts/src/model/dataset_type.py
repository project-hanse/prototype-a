from enum import Enum


class DatasetType(Enum):
	File = 0
	PdSeries = 1
	PdDataFrame = 2
	StaticPlot = 3
	Prophet = 4
	SklearnModel = 5
	SklearnEncoder = 6
