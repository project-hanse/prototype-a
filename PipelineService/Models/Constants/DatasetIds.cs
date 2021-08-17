using System;

namespace PipelineService.Models.Constants
{
    public static class DatasetIds
    {
        public static readonly Guid DsIdMelbourneHousePricesLess = Guid.Parse("0c2acbdb-544b-4efc-ae54-c2dcba988654");
        public static readonly Guid DsIdInfluencaVienna20092018 = Guid.Parse("4cfd0698-004a-404e-8605-de2f830190f2");
        public static readonly Guid DsIdChemnitzBerufsbildung1993 = Guid.Parse("2b88720f-8d2d-46c8-84d2-ab177c88cb5f");
        public static readonly Guid DsIdChemnitzStudenten1993 = Guid.Parse("61501213-d945-49a5-9212-506d6305af13");
        public static readonly Guid DsIdSimulatedVineYield = Guid.Parse("1a953cb2-4ad1-4c07-9a80-bd2c6a68623a");

        public static Guid ZamgWeatherId(int year)
        {
            return Guid.Parse($"8d15d14d-2eba-4d36-b2ba-aaaaaaaa{year}");
        }
    }
}