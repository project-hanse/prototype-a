using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Models.Pipeline;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class NodesServiceUnitTests
    {
        private IPipelinesDaoInMemory _pipelinesDaoInMemory;
        private INodesService _nodesService;

        [SetUp]
        public void Setup()
        {
            _pipelinesDaoInMemory = new InMemoryPipelinesDaoInMemory(GeneralHelper.CreateLogger<InMemoryPipelinesDaoInMemory>());
            _nodesService = new NodesService(GeneralHelper.CreateLogger<NodesService>(), _pipelinesDaoInMemory);
        }

        [Test]
        public async Task FindNodeOrDefault_PipelineDoesNotExist_ReturnsDefault()
        {
            // arrange

            // act
            var node = await _nodesService.FindNodeOrDefault(Guid.NewGuid(), Guid.NewGuid());

            // assert
            Assert.IsNull(node);
        }

        [Test]
        public async Task FindNodeOrDefault_PipelineExistsNodeDoesNotExist_ReturnsDefault()
        {
            // arrange
            var pipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline();
            await _pipelinesDaoInMemory.Add(pipeline);

            // act
            var node = await _nodesService.FindNodeOrDefault(pipeline.Id, Guid.NewGuid());

            // assert
            Assert.IsNull(node);
        }

        [Test]
        public async Task FindNodeOrDefault_PipelineExistsNodeExists_IdsMatch()
        {
            // arrange
            var pipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline();
            var node = pipeline.Root[0].Successors[0];
            await _pipelinesDaoInMemory.Add(pipeline);

            // act
            var result = await _nodesService.FindNodeOrDefault(pipeline.Id, node.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(node.Id, result.Id);
        }

        [Test]
        public async Task FindNodeOrDefault_PipelineExistsNodeExists_FindsNestedNode()
        {
            // arrange
            var pipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline();

            var node = new NodeSingleInput
            {
                PipelineId = pipeline.Id,
                Operation = "some_operation"
            };

            pipeline.Root[0].Successors.Add(node);

            await _pipelinesDaoInMemory.Add(pipeline);

            // act
            var result = await _nodesService.FindNodeOrDefault(pipeline.Id, node.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(node.Id, result.Id);
        }
    }
}