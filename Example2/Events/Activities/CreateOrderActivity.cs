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
    public static class CreateOrderActivity
    {
        [FunctionName("Example2-Events-CreateOrder")]
        [return: ServiceBus("create-order", Connection = "ServiceBus", EntityType = EntityType.Queue)]
        public static Message CreateOrder([ActivityTrigger] IDurableActivityContext context)
        {
            var command = context.GetInput<CreateOrderCommand>();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            
            return new Message(bytes)
            {
                ReplyTo = context.InstanceId
            };
        }

        [FunctionName("Example2-Events-OrderCreatedHandler")]
        public static async Task HandleOrderCreated(
            [ServiceBusTrigger("order-created", "orchestrator", Connection = "ServiceBus")] Message message,
            [DurableClient] IDurableOrchestrationClient client)
        {
            var @event = JsonConvert.DeserializeObject<OrderCreatedEvent>(Encoding.UTF8.GetString(message.Body));

            await client.RaiseEventAsync(message.ReplyTo, "order-created", @event);
        }
    }
}
