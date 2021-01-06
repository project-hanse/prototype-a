using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class PipelineExecutionUnitTests
    {
        private IPipelineExecutionService _pipelineExecutionService;
        private IPipelineService _pipelineService;
        private IMqttMessageService _mqttMessageService;

        [SetUp]
        public void SetUp()
        {
            var pipelineServiceMock = new Mock<IPipelineService>();
            pipelineServiceMock
                .Setup(p => p.GetPipeline(It.IsAny<Guid>()))
                .Returns<Guid>((pipelineId) => Task.FromResult(ModelHelper.NewDefaultPipeline(pipelineId)));
            _pipelineService = pipelineServiceMock.Object;

            var mqttServiceMock = new Mock<IMqttMessageService>();
            _mqttMessageService = mqttServiceMock.Object;

            _pipelineExecutionService = new PipelineExecutionService(
                GeneralHelper.CreateLogger<PipelineExecutionService>(),
                _pipelineService,
                _mqttMessageService);
        }
    }
}