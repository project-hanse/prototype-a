using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PipelineService.Helper
{
	public static class HashHelper
	{
		public static string ComputeHash(params object[] args)
		{
			return Sha256(string.Join("|", JsonSerializer.Serialize(args)));
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
