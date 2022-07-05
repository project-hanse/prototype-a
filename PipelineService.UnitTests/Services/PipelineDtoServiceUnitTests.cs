using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Models;
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
			var mockOperationsService = new Mock<IOperationsService>();

			mockPipelineDao.Setup(s => s.CreatePipeline(It.IsAny<Pipeline>()))
				.Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateRootOperation(It.IsAny<Guid>(), It.IsAny<Operation>()))
				.Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateSuccessor(It.IsAny<List<Guid>>(), It.IsAny<Operation>()))
				.Returns(Task.CompletedTask);

			var mockHttpClientFactory = new Mock<IHttpClientFactory>();
			mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
			var pipelinesDtoService = new PipelinesDtoService(
				GeneralHelper.CreateLogger<PipelinesDtoService>(),
				GeneralHelper.Configuration(new Dictionary<string, string>()
				{
					{ "DefaultPipelinesFolder", Path.Combine("Resources", "DefaultPipelines") },
					{ "PipelineCandidatesFolder", Path.Combine("Resources", "PipelineCandidates") }
				}),
				mockPipelineDao.Object,
				null,
				null,
				mockOperationsService.Object,
				null,
				mockHttpClientFactory.Object
			);


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
	}
}
