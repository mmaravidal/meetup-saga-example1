using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace Example2
{
    public static class LeaseStockActivity
    {
        [FunctionName("Example2-Events-LeaseStock")]
        [return: ServiceBus("lease-stock", Connection = "%ServiceBus%", EntityType = EntityType.Queue)]
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
            [ServiceBus("stock-leased", Connection = "%ServiceBus%", EntityType = EntityType.Topic)] StockLeasedEvent @event,
            [DurableClient] IDurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(@event.Id, "stock-leased", @event);
        }

        [FunctionName("Example2-Events-StockLeaseDeniedHandler")]
        public static async Task HandleStockLeaseDenied(
            [ServiceBus("stock-lease-denied", Connection = "%ServiceBus%", EntityType = EntityType.Topic)] StockLeaseDeniedEvent @event,
            [DurableClient] IDurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(@event.Id, "order-create-denied", @event);
        }
    }
}
