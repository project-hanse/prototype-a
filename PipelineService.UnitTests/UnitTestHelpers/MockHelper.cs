using Moq;
using PipelineService.Dao;
using PipelineService.Services;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class MockHelper
    {
        public static IPipelinesDao PipelineServiceMock()
        {
            var mock = new Mock<IPipelinesDao>();

            return mock.Object;
        }
    }
}