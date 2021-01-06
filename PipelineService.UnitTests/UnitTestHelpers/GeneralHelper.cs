using Microsoft.Extensions.Caching.Memory;
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
    }
}