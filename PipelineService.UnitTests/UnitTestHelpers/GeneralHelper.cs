using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PipelineService.UnitTests.UnitTestHelpers
{
	public static class GeneralHelper
	{
		/// <summary>
		/// Creates a logger for a given type.
		/// </summary>
		public static ILogger<T> CreateLogger<T>()
		{
			return new Logger<T>(new LoggerFactory());
		}

		/// <summary>
		/// Creates a new InMemoryCache.
		/// </summary>
		public static IMemoryCache CreateInMemoryCache()
		{
			return new MemoryCache(new MemoryCacheOptions());
		}

		public static IConfiguration EmptyConfiguration()
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string>())
				.Build();
		}

		public static IConfiguration Configuration(Dictionary<string, string> dictionary)
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(dictionary)
				.Build();
		}
	}
}
