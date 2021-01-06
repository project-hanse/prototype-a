using Moq;
using PipelineService.Services;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class MockHelper
    {
        public static IPipelineService PipelineServiceMock()
        {
            var mock = new Mock<IPipelineService>();

            return mock.Object;
        }
    }
}