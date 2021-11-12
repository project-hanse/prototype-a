using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PipelineService.Extensions
{
	public static class HttpContextExtensions
	{
		public static string GetUsernameFromBasicAuthHeader(this HttpContext httpContext)
		{
			var authHeader = AuthenticationHeaderValue.Parse(httpContext.Request.Headers["Authorization"]);
			var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? throw new InvalidOperationException());
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
			return credentials[0];
		}
	}
}
