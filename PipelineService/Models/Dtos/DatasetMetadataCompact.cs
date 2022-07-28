using System;
using Newtonsoft.Json;

namespace PipelineService.Models.Dtos;

public class DatasetMetadataCompact
{
	/// <summary>
	/// The data type this dataset is stored as.
	/// </summary>
	/// <remarks>
	/// Examples are: str, series, dataframe, etc.
	/// </remarks>
	[JsonProperty("type")]
	public string Type { get; set; }

	/// <summary>
	/// The type of model of this dataset.
	/// </summary>
	/// <remarks>
	/// Only used for sklearn models.
	/// Examples are: decisiontreeclassifier, decisiontreeregressor, etc.
	/// </remarks>
	[JsonProperty("model_type")]
	public string ModelType { get; set; }

	/// <summary>
	/// The column names of this dataset.
	/// </summary>
	/// <remarks>
	/// Only used for dataframes.
	/// </remarks>
	[JsonProperty("columns")]
	public string[] Columns { get; set; } = Array.Empty<string>();
}
