using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System.Text;
using Example2.Events.Models;

namespace Example2
{
    public static class LeaseStockActivity
    {
        [FunctionName("Example2-Events-LeaseStock")]
        [return: ServiceBus("lease-stock", Connection = "ServiceBus", EntityType = EntityType.Queue)]
        public static Message LeaseStock([ActivityTrigger] IDurableActivityContext context)
        {
            var command = context.GetInput<LeaseStockCommand>();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));

            return new Message(bytes)
            {
                ReplyTo = context.InstanceId
            };
        }

        [FunctionName("Example2-Events-StockLeasedHandler")]
        public static async Task HandleStockLeased(
            [ServiceBusTrigger("stock-leased", "orchestrator", Connection = "ServiceBus")] Message message,
            [DurableClient] IDurableOrchestrationClient client)
        {
            var @event = JsonConvert.DeserializeObject<StockLeasedEvent>(Encoding.UTF8.GetString(message.Body));

            await client.RaiseEventAsync(message.ReplyTo, "stock-leased", @event);
        }

        [FunctionName("Example2-Events-StockLeaseDeniedHandler")]
        public static async Task HandleStockLeaseDenied(
            [ServiceBusTrigger("stock-lease-denied", "orchestrator", Connection = "ServiceBus")] Message message,
            [DurableClient] IDurableOrchestrationClient client)
        {
            var @event = JsonConvert.DeserializeObject<StockLeaseDeniedEvent>(Encoding.UTF8.GetString(message.Body));

            await client.RaiseEventAsync(message.ReplyTo, "stock-lease-denied", @event);
        }
    }
}
