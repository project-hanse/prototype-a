using Hangfire.Dashboard;

namespace PipelineService.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context)
	{
		// Allow all traffic for now since api is deployed behind proxy that does authentication
		return true;
	}
}
