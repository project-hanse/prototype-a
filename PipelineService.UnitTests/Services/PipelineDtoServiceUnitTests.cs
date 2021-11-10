using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class PipelineDtoServiceUnitTests
    {
        // private static NodeTupleTestCase[] _getNodeTupleTestCases =
        // {
        //     new()
        //     {
        //         Pipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline(),
        //         ExpectedSingleInputTuples = 3,
        //         ExpectedDoubleInputTuples = 0
        //     },
        //     new()
        //     {
        //         Pipeline = HardcodedDefaultPipelines.InfluenzaInterpolation(),
        //         ExpectedSingleInputTuples = 3,
        //         ExpectedDoubleInputTuples = 0
        //     },
        //     new()
        //     {
        //         Pipeline = HardcodedDefaultPipelines.ChemnitzStudentAndJobsPipeline(),
        //         ExpectedSingleInputTuples = 13,
        //         ExpectedDoubleInputTuples = 1
        //     }
        // };
        //
        // private IPipelinesDaoInMemory _pipelinesDaoInMemory;
        // private IPipelinesExecutionDao _pipelinesExecutionDao;
        // private IPipelinesDtoService _pipelinesDtoService;
        //
        // [SetUp]
        // public void SetUp()
        // {
        //     var mock = new Mock<IPipelinesExecutionDao>();
        //     mock.Setup(dao => dao.GetLastExecutionForPipeline(It.IsAny<Guid>()))
        //         .ReturnsAsync((Guid pipelineId) => new PipelineExecutionRecord
        //         {
        //             PipelineId = pipelineId,
        //             CompletedOn = DateTime.UtcNow
        //         });
        //     _pipelinesExecutionDao = mock.Object;
        //
        //     _pipelinesDaoInMemory = new InMemoryPipelinesDaoInMemory(GeneralHelper.CreateLogger<InMemoryPipelinesDaoInMemory>());
        //     _pipelinesDtoService = new PipelinesDtoService(_pipelinesDaoInMemory, _pipelinesExecutionDao);
        // }
        //
        // [Test]
        // [TestCaseSource(nameof(_getNodeTupleTestCases))]
        // public async Task GetSingleInputNodeTuples_ExecutedPipeline(NodeTupleTestCase testCase)
        // {
        //     // arrange
        //     await _pipelinesDaoInMemory.Add(testCase.Pipeline);
        //
        //     // act
        //     var results = await _pipelinesDtoService.GetSingleInputNodeTuples(testCase.Pipeline.Id);
        //
        //     // assert
        //     Assert.NotNull(results);
        //     Assert.AreEqual(testCase.ExpectedSingleInputTuples, results.Count);
        // }
    }
    
    public class NodeTupleTestCase
    {
        public Pipeline Pipeline { get; set; }
        public int ExpectedSingleInputTuples { get; set; }
        public int ExpectedDoubleInputTuples { get; set; }
    }
}