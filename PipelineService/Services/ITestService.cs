using System.Threading.Tasks;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services
{
    public interface ITestService
    {
        Task NewMessage(BlockExecutionResponse message);
    }
}