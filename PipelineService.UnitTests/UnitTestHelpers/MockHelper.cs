using Moq;
using PipelineService.Dao;
using PipelineService.Services;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class MockHelper
    {
        public static IPipelinesDaoInMemory PipelineServiceMock()
        {
            var mock = new Mock<IPipelinesDaoInMemory>();

            return mock.Object;
        }
    }
}