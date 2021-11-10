using PipelineService.Helper;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class HashService : IHashService
    {
        public string ComputeHash(Node node)
        {
            return HashHelper.ComputeStaticHash(node);
        }
    }
}