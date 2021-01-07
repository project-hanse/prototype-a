using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PipelineService.Models.Pipeline;

namespace PipelineService.Helper
{
    public static class HashHelper
    {
        public static string ComputeStaticHash(Block block)
        {
            if (block == null)
            {
                throw new NullReferenceException();
            }

            var inputBuilder = new StringBuilder();

            inputBuilder.Append(JsonSerializer.Serialize(block.PipelineId));
            inputBuilder.Append(JsonSerializer.Serialize(block.Operation));
            inputBuilder.Append(JsonSerializer.Serialize(block.OperationConfiguration));
            inputBuilder.Append(JsonSerializer.Serialize(block.IncludeInHash));

            using var hash = SHA256.Create();

            var enc = Encoding.UTF8;

            var result = hash.ComputeHash(enc.GetBytes(inputBuilder.ToString()));

            var hashBuilder = new StringBuilder();
            foreach (var b in result)
            {
                hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }
    }
}