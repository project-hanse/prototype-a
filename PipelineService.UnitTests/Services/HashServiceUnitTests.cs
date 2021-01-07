using System;
using System.Collections.Generic;
using NUnit.Framework;
using PipelineService.Models.Pipeline;
using PipelineService.Services;
using PipelineService.Services.Impl;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class HashServiceUnitTests
    {
        private IHashService _hashService;

        [SetUp]
        public void Setup()
        {
            _hashService = new HashService();
        }

        [Test]
        public void ComputeHash_NullParameter_ShouldThrow()
        {
            Assert.Throws<NullReferenceException>(() => _hashService.ComputeHash(null));
        }

        [Test]
        public void ComputeHash_WithInputDatasetId_ShouldComputeSameHash()
        {
            // arrange
            var simpleBlock1 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetId = Guid.Parse("7a26d703-1942-42fd-8b46-54bac5bfc988"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            var simpleBlock2 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetId = Guid.Parse("7a26d703-1942-42fd-8b46-54bac5bfc988"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            // act
            var result1 = _hashService.ComputeHash(simpleBlock1);
            var result2 = _hashService.ComputeHash(simpleBlock2);

            // assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void ComputeHash_WithInputDatasetHash_ShouldComputeSameHash()
        {
            // arrange
            var simpleBlock1 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetHash = "21e90095cce54b21fc55067f904d8e2d61d869956d363efa3d2c9d7a5419b408",
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            var simpleBlock2 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetHash = "21e90095cce54b21fc55067f904d8e2d61d869956d363efa3d2c9d7a5419b408",
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            // act
            var result1 = _hashService.ComputeHash(simpleBlock1);
            var result2 = _hashService.ComputeHash(simpleBlock2);

            // assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void ComputeHash_WithInputDatasetId_ShouldComputeDifferentHashes()
        {
            // arrange
            var simpleBlock1 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetId = Guid.Parse("7a26d703-1942-42fd-8b46-54bac5bfc988"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            var simpleBlock2 = new SimpleBlock
            {
                PipelineId = Guid.Parse("81c21ab9-dd6d-41e0-bf78-9c87d05a7188"),
                InputDatasetId = Guid.Parse("37f245f4-7b93-4ec9-9da9-c8f042b40df9"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                }
            };

            // act
            var result1 = _hashService.ComputeHash(simpleBlock1);
            var result2 = _hashService.ComputeHash(simpleBlock2);

            // assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.AreNotEqual(result1, result2);
        }
    }
}