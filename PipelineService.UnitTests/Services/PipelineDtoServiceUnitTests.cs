using System.Collections.Generic;
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
    public class PipelineDtoServiceUnitTests
    {
        private IPipelineDao _pipelineDao;
        private IPipelineDtoService _pipelineDtoService;

        [SetUp]
        public void SetUp()
        {
            _pipelineDao = new InMemoryPipelineDao(GeneralHelper.CreateLogger<InMemoryPipelineDao>());
            _pipelineDtoService = new PipelineDtoService(_pipelineDao);
        }

        [Test]
        public async Task GetSingleInputNodeTuples_OnlySingleInputs()
        {
            // arrange
            await _pipelineDao.CreateDefaults(
                new List<Pipeline> { HardcodedDefaultPipelines.MelbourneHousingPipeline() });

            // act
            var results = await _pipelineDtoService.GetSingleInputNodeTuples();

            // assert
            Assert.NotNull(results);
        }
    }
}