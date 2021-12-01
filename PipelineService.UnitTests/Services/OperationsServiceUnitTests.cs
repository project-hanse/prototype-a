using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using PipelineService.Models.Constants;
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
                GeneralHelper.EmptyConfiguration());
        }

        [Test]
        public async Task GetOperations_ReturnsAllOperations()
        {
            // act
            var operations = await _operationTemplatesService.GetOperationDtos();

            // assert
            Assert.NotNull(operations);
            Assert.GreaterOrEqual(typeof(OperationIds).GetFields(BindingFlags.Static | BindingFlags.Public).Length,
                operations.Count);
        }
    }
}