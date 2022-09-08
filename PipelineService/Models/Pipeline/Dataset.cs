using System;
using System.ComponentModel.DataAnnotations;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Pipeline
{
	public record Dataset
	{
		/// <summary>
		/// The dataset's type.
		/// </summary>
		public DatasetType Type { get; set; }

		/// <summary>
		/// The dataset's key used to load and store the dataset.
		/// This should be an arbitrary string that uniquely identifies the dataset within its store.
		/// </summary>
		[Key]
		public string Key { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// A string that identifies the data store the dataset is held in.
		/// Could be for example an S3 bucket or a store specific to a dataset type.
		/// </summary>
		public string Store { get; set; }
	}
}
