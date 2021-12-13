using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
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
			_operationTemplatesService = new OperationTemplatesService(
				GeneralHelper.CreateLogger<OperationTemplatesService>(),
				GeneralHelper.Configuration(new Dictionary<string, string>()
				{
					{ "OperationTemplatesFolder", "Resources/OperationTemplates" }
				}),
				GeneralHelper.CreateInMemoryCache());
		}

		[Test]
		public async Task GetOperations_ReturnsAllOperations()
		{
			// act
			var operations = await _operationTemplatesService.GetOperationDtos();

			// assert
			Assert.NotNull(operations);
			Assert.GreaterOrEqual(operations.Count, 0);
		}
	}
}
