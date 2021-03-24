using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class PipelineExecutionServiceUnitTests
    {
        private IPipelineExecutionService _pipelineExecutionService;
        private IPipelineDao _pipelineDao;
        private IPipelineExecutionDao _pipelineExecutionDao;
        private EventBusService _eventBusService;
        private EdgeEventBusService _edgeEventBusService;

        [SetUp]
        public void SetUp()
        {
            var pipelineDaoMock = new Mock<IPipelineDao>();
            pipelineDaoMock
                .Setup(p => p.Get(It.IsAny<Guid>()))
                .Returns<Guid>(pipelineId => Task.FromResult(ModelHelper.NewDefaultPipeline(pipelineId)));
            _pipelineDao = pipelineDaoMock.Object;

            _pipelineExecutionDao =
                new InMemoryPipelineExecutionDao(GeneralHelper.CreateLogger<InMemoryPipelineExecutionDao>());

            var mqttServiceMock = new Mock<EventBusService>();
            _eventBusService = mqttServiceMock.Object;
            var edgeMqttServiceMock = new Mock<EdgeEventBusService>();
            _edgeEventBusService = edgeMqttServiceMock.Object;

            _pipelineExecutionService = new PipelineExecutionService(
                GeneralHelper.CreateLogger<PipelineExecutionService>(),
                _pipelineDao,
                _pipelineExecutionDao,
                _eventBusService,
                _edgeEventBusService);
        }
    }
}