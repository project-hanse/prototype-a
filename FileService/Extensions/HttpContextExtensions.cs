using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace FileService.Extensions
{
	public static class HttpContextExtensions
	{
		// Same class exists in PipelinesService - TODO: create common library and use it in CI
		public static string GetUsernameFromBasicAuthHeader(this HttpContext httpContext)
		{
			if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValue)) return null;

			var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue);
			var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? throw new InvalidOperationException());
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
			return credentials[0];
		}
	}
}
