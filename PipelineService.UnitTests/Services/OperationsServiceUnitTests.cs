using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
	[TestFixture]
	public class OperationsServiceUnitTests
	{
		private IOperationTemplatesService _operationTemplatesService;

		[SetUp]
		public void Setup()
		{
			var pipelineDaoMock = new Mock<IPipelinesDao>();
			pipelineDaoMock.Setup(s => s.GetUsedOperationIdentifiers(It.IsAny<bool>()))
				.ReturnsAsync(() => new List<string>()
				{
					"0759dede-2cee-433c-b314-10a8fa456e62-join", // pd.join
					"0759dede-2cee-433c-b314-10a8fa456e62-dropna" // pd.dropna
				});
			_operationTemplatesService = new OperationTemplatesService(
				GeneralHelper.CreateLogger<OperationTemplatesService>(),
				GeneralHelper.Configuration(new Dictionary<string, string>()
				{
					{ "OperationTemplatesFolder", "Resources/OperationTemplates" }
				}),
				pipelineDaoMock.Object,
				GeneralHelper.CreateInMemoryCache());
		}

		[Test]
		public async Task GetOperations_FilterOperation_ReturnsAllOperations()
		{
			// arrange
			var request = new GetOperationTemplatesRequest()
			{
				FilterUnused = true
			};

			// act
			var operations = await _operationTemplatesService.GetOperationDtos(request);

			// assert
			Assert.NotNull(operations);
			Assert.GreaterOrEqual(operations.Count, 0);
		}

		[Test]
		public async Task GetOperations_DontFilterOperation_ReturnsAllOperations()
		{
			// arrange
			var request = new GetOperationTemplatesRequest()
			{
				FilterUnused = false
			};

			// act
			var operations = await _operationTemplatesService.GetOperationDtos(request);

			// assert
			Assert.NotNull(operations);
			Assert.GreaterOrEqual(operations.Count, 0);
		}
	}
}
