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
        // private IPipelineExecutionService _pipelineExecutionService;
        // private IPipelinesDaoInMemory _pipelinesDaoInMemory;
        // private IPipelinesExecutionDao _pipelinesExecutionDao;
        // private EventBusService _eventBusService;
        // private EdgeEventBusService _edgeEventBusService;
        //
        // [SetUp]
        // public void SetUp()
        // {
        //     var pipelineDaoMock = new Mock<IPipelinesDaoInMemory>();
        //     pipelineDaoMock
        //         .Setup(p => p.Get(It.IsAny<Guid>()))
        //         .Returns<Guid>(pipelineId => Task.FromResult(ModelHelper.NewDefaultPipeline(pipelineId)));
        //     _pipelinesDaoInMemory = pipelineDaoMock.Object;
        //
        //     _pipelinesExecutionDao =
        //         new InMemoryPipelinesExecutionDao(GeneralHelper.CreateLogger<InMemoryPipelinesExecutionDao>());
        //
        //     var mqttServiceMock = new Mock<EventBusService>();
        //     _eventBusService = mqttServiceMock.Object;
        //     var edgeMqttServiceMock = new Mock<EdgeEventBusService>();
        //     _edgeEventBusService = edgeMqttServiceMock.Object;
        //
        //     _pipelineExecutionService = new PipelinesExecutionService(
        //         GeneralHelper.CreateLogger<PipelinesExecutionService>(),
        //         _pipelinesDao,
        //         _pipelinesExecutionDao,
        //         _eventBusService,
        //         _edgeEventBusService);
        // }
    }
}