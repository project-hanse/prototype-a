using System;

namespace PipelineService.Models.Constants
{
    public static class DatasetIds
    {
        public static readonly Guid DsIdMelbourneHousePricesLess = Guid.Parse("0c2acbdb-544b-4efc-ae54-c2dcba988654");
        public static readonly Guid DsIdSimulatedVineYield = Guid.Parse("1a953cb2-4ad1-4c07-9a80-bd2c6a68623a");

        public static Guid ZamgWeatherId(int year)
        {
            return Guid.Parse($"8d15d14d-2eba-4d36-b2ba-aaaaaaaa{year}");
        }
    }
}