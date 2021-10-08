using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Exceptions;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class PipelineExecutionDaoUnitTests
    {
        private IPipelinesExecutionDao _pipelinesExecutionDao;

        [SetUp]
        public void SetUp()
        {
            _pipelinesExecutionDao =
                new InMemoryPipelinesExecutionDao(GeneralHelper.CreateLogger<InMemoryPipelinesExecutionDao>());
        }

        [Test]
        public async Task CreateExecution_ShouldCreateNew()
        {
            // arrange
            var pipeline = ModelHelper.NewDefaultPipeline();

            // act
            var result = await _pipelinesExecutionDao.Create(pipeline);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual(pipeline.Id, result.PipelineId);
            Assert.NotNull(result.ToBeExecuted);
            Assert.AreEqual(5, result.ToBeExecuted.Count);
        }


        [Test]
        public void GetById_NoExecutionExists_ShouldThrow()
        {
            Assert.ThrowsAsync<NotFoundException>(async () => await _pipelinesExecutionDao.Get(Guid.NewGuid()));
        }

        [Test]
        public async Task GetById_ShouldReturnRecord()
        {
            // arrange
            var pipeline = ModelHelper.NewDefaultPipeline();
            var executionRecord = await _pipelinesExecutionDao.Create(pipeline);

            // act
            var result = await _pipelinesExecutionDao.Get(executionRecord.Id);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual(executionRecord.Id, result.Id);
        }
    }
}