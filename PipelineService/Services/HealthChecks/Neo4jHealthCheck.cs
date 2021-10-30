using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Neo4jClient;

namespace PipelineService.Services.HealthChecks
{
	public class Neo4JHealthCheck : IHealthCheck
	{
		private readonly IGraphClient _graphClient;
		private readonly ILogger<Neo4JHealthCheck> _logger;

		public Neo4JHealthCheck(IGraphClient graphClient, ILogger<Neo4JHealthCheck> logger)
		{
			_graphClient = graphClient;
			_logger = logger;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
			CancellationToken cancellationToken = new())
		{
			if (!_graphClient.IsConnected)
			{
				_logger.LogDebug("Connecting to neo4j for health check");
				await _graphClient.ConnectAsync();
			}

			try
			{
				// TODO change to pipeline
				var query = _graphClient.Cypher
					.Match("(c:Company)")
					.Return(c => c.As<string>())
					.Limit(1);

				var text = query.Query.DebugQueryText;

				_logger.LogDebug("Sending health check query to neo4j: {HealthCheckQuery}", text);

				await query.ResultsAsync;

				return HealthCheckResult.Healthy();
			}
			catch (Exception e)
			{
				return HealthCheckResult.Unhealthy(e.Message);
			}
		}
	}
}
