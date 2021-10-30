using System;
using Microsoft.Extensions.Configuration;

namespace PipelineService.Extensions
{
	public static class IConfigurationExtensions
	{
		public static T GetValueOrThrow<T>(this IConfiguration configuration, string key)
		{
			return configuration.GetValue<T>(key) ??
			       throw new InvalidOperationException($"Missing mandatory configuration key '{key}'");
		}
	}
}
