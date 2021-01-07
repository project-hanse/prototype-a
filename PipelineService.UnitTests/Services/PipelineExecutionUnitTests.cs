using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Exceptions;
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

        [Test]
        public async Task CreateExecution_ShouldCreateNew()
        {
            // arrange
            var pipeline = ModelHelper.NewDefaultPipeline();

            // act
            var result = await _pipelineExecutionService.CreateExecution(pipeline);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual(pipeline.Id, result.PipelineId);
            Assert.NotNull(result.ToBeExecuted);
            Assert.AreEqual(5, result.ToBeExecuted.Count);
        }

        [Test]
        public void GetById_NoExecutionExists_ShouldThrow()
        {
            Assert.ThrowsAsync<NotFoundException>(async () => await _pipelineExecutionService.GetById(Guid.NewGuid()));
        }

        [Test]
        public async Task GetById_ShouldReturnRecord()
        {
            // arrange
            var pipeline = ModelHelper.NewDefaultPipeline();
            var executionRecord = await _pipelineExecutionService.CreateExecution(pipeline);

            // act
            var result = await _pipelineExecutionService.GetById(executionRecord.Id);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual(executionRecord.Id, result.Id);
        }
    }
}