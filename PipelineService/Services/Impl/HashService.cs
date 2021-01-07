using PipelineService.Helper;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class HashService : IHashService
    {
        public string ComputeHash(Block block)
        {
            return HashHelper.ComputeStaticHash(block);
        }
    }
}