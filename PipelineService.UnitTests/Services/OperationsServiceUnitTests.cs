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
        private IOperationsService _operationsService;

        [SetUp]
        public void Setup()
        {
            _operationsService = new OperationsService(
                GeneralHelper.CreateLogger<OperationsService>(),
                GeneralHelper.EmptyConfiguration());
        }

        [Test]
        public async Task GetOperations_ReturnsAllOperations()
        {
            // act
            var operations = await _operationsService.GetOperationDtos();

            // assert
            Assert.NotNull(operations);
            Assert.GreaterOrEqual(typeof(OperationIds).GetFields(BindingFlags.Static | BindingFlags.Public).Length,
                operations.Count);
        }
    }
}