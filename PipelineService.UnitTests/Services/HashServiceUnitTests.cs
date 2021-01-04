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
        public void ComputeHash_WithInputDatasetId_ShouldComputeHash()
        {
            // arrange
            var simpleBlock = new SimpleBlock
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
            var result = _hashService.ComputeHash(simpleBlock);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual("5462eb5dd7d6d92d8039a332209a33364505956470885270a2a42681406c8dd9", result);
        }
        
        [Test]
        public void ComputeHash_WithInputDatasetHash_ShouldComputeHash()
        {
            // arrange
            var simpleBlock = new SimpleBlock
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
            var result = _hashService.ComputeHash(simpleBlock);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual("dbc9309b255684f2da2a51c904df2e15ab2bf5d2921ad4dcee1fce72e44e46fe", result);
        }
    }
}