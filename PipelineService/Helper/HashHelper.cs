using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PipelineService.Models.Pipeline;

namespace PipelineService.Helper
{
    public static class HashHelper
    {
        public static string ComputeHash(params object[] args)
        {
            return Sha256(string.Join("|", JsonSerializer.Serialize(args)));
        }

        public static string ComputeStaticHash(Node node)
        {
            if (node == null)
            {
                throw new NullReferenceException();
            }

            var inputBuilder = new StringBuilder();

            inputBuilder.Append(JsonSerializer.Serialize(node.PipelineId));
            inputBuilder.Append(JsonSerializer.Serialize(node.Operation));
            inputBuilder.Append(JsonSerializer.Serialize(node.OperationConfiguration));
            inputBuilder.Append(JsonSerializer.Serialize(node.IncludeInHash));

            var input = inputBuilder.ToString();

            return Sha256(input);
        }

        private static string Sha256(string input)
        {
            using var hash = SHA256.Create();

            var enc = Encoding.UTF8;

            var result = hash.ComputeHash(enc.GetBytes(input));

            var hashBuilder = new StringBuilder();
            foreach (var b in result)
            {
                hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }
    }
}