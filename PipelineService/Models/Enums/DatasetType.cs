namespace PipelineService.Models.Enums
{
	public enum DatasetType
	{
		File = 0,
		PdSeries = 1,
		PdDataFrame = 2,
		StaticPlot = 3,
		Prophet = 4,
		SklearnModel = 5,
		SklearnEncoder = 6,
		Dict = 7,
		DictVectorizer = 8,
		NpArray = 9,
	}
}
