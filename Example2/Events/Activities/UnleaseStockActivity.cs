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
    public static class UnleaseStockActivity
    {
        [FunctionName("Example2-Events-UnleaseStock")]
        [return: ServiceBus("unlease-stock", Connection = "ServiceBus", EntityType = EntityType.Queue)]
        public static Message UnleaseStock([ActivityTrigger] IDurableActivityContext context)
        {
            var command = context.GetInput<UnleaseStockCommand>();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));

            return new Message(bytes)
            {
                ReplyTo = context.InstanceId
            };
        }

        [FunctionName("Example2-Events-StockUnleasedHandler")]
        public static async Task HandleStockLeased(
            [ServiceBus("stock-unleased", Connection = "ServiceBus", EntityType = EntityType.Topic)] StockLeasedEvent @event,
            [DurableClient] IDurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(@event.Id, "stock-unleased", @event);
        }
    }
}
