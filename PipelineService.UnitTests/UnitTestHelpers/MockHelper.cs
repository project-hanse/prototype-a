using Moq;
using PipelineService.Dao;
using PipelineService.Services;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class MockHelper
    {
        public static IPipelineDao PipelineServiceMock()
        {
            var mock = new Mock<IPipelineDao>();

            return mock.Object;
        }
    }
}