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

		private static string[] _pipelineCandidateFileNames =
		{
			"pipeline-1652383756.json",
			"pipeline-1652384137.json",
			"pipeline-1652384281.json"
		};

		private string _defaultPipelinesPath;
		private string _pipelineCandidatesPath;
		private IPipelinesDtoService _pipelinesDtoService;

		[SetUp]
		public void Setup()
		{
			// mock IPipelineDao
			var mockPipelineDao = new Mock<IPipelinesDao>();

			mockPipelineDao.Setup(s => s.CreatePipeline(It.IsAny<Pipeline>())).Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateRootOperation(It.IsAny<Guid>(), It.IsAny<Operation>()))
				.Returns(Task.CompletedTask);
			mockPipelineDao.Setup(s => s.CreateSuccessor(It.IsAny<List<Guid>>(), It.IsAny<Operation>()))
				.Returns(Task.CompletedTask);

			var pipelinesDtoService = new PipelinesDtoService(
				GeneralHelper.CreateLogger<PipelinesDtoService>(),
				GeneralHelper.Configuration(new Dictionary<string, string>()
				{
					{ "DefaultPipelinesFolder", Path.Combine("Resources", "DefaultPipelines") },
					{ "PipelineCandidatesFolder", Path.Combine("Resources", "PipelineCandidates") }
				}),
				mockPipelineDao.Object);

			_defaultPipelinesPath = pipelinesDtoService.DefaultPipelinesPath;
			_pipelineCandidatesPath = pipelinesDtoService.PipelineCandidatesPath;
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

		[Test]
		[TestCaseSource(nameof(_pipelineCandidateFileNames))]
		public async Task ImportPipelineCandidate_ShouldGeneratePipelineFromCandidate_ReturnPipelineGuid(
			string pipelineFileName)
		{
			// arrange
			var pipelineCandidate =
				JsonConvert.DeserializeObject<PipelineCandidate>(
					await File.ReadAllTextAsync(
						Path.Combine(_pipelineCandidatesPath, pipelineFileName)));
			Assert.NotNull(pipelineCandidate, "Fail to load pipeline candidate from file");

			// act
			var pipelineId = await _pipelinesDtoService.ImportPipelineCandidate(pipelineCandidate);

			// assert
			Assert.NotNull(pipelineId);
			Assert.AreNotEqual(pipelineCandidate.PipelineId, pipelineId);
			Assert.AreNotEqual(Guid.Empty, pipelineId);
		}
	}
}
