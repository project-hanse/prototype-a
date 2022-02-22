using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
	[TestFixture]
	public class PipelineDtoServiceUnitTests
	{
		private static string[] _pipelineFileNames =
		{
			"beer-australia.json"
		};

		private string _defaultPipelinesPath;
		private IPipelinesDtoService _pipelinesDtoService;

		[SetUp]
		public void Setup()
		{
			// mock IPipelineDao
			var mockPipelineDao = new Mock<IPipelinesDao>();

			mockPipelineDao.Setup(s => s.CreatePipeline(It.IsAny<Pipeline>())).Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateRootOperation(It.IsAny<Guid>(), It.IsAny<Operation>())).Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateSuccessor(It.IsAny<List<Guid>>(), It.IsAny<Operation>())).Returns(Task.CompletedTask);

			var pipelinesDtoService = new PipelinesDtoService(
				GeneralHelper.CreateLogger<PipelinesDtoService>(),
				GeneralHelper.Configuration(new Dictionary<string, string>()
				{
					{ "DefaultPipelinesFolder", "Resources/DefaultPipelines" }
				}),
				mockPipelineDao.Object);

			_defaultPipelinesPath = pipelinesDtoService.DefaultPipelinesPath;
			_pipelinesDtoService = pipelinesDtoService;
		}

		[Test]
		[TestCaseSource(nameof(_pipelineFileNames))]
		public async Task ImportPipeline_ShouldParseFile_ReturnPipelineGuid(string pipelineFileName)
		{
			// arrange
			var pipelineExport =
				JsonConvert.DeserializeObject<PipelineExport>(
					await File.ReadAllTextAsync(
						Path.Combine(_defaultPipelinesPath, pipelineFileName)));
			Assert.NotNull(pipelineExport, "Failed to load pipeline from file");

			// act
			var pipelineId = await _pipelinesDtoService.ImportPipeline(pipelineExport);

			// assert
			Assert.NotNull(pipelineId);
			Assert.AreNotEqual(pipelineExport.PipelineId, pipelineId);
			Assert.AreNotEqual(Guid.Empty, pipelineId);
		}

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
