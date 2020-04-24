using Example2.Events.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Example2.Events.Fakes
{
    public class OrderService
    {
        [FunctionName("Example2-Events-FakeStockService-CreateOrderHandler")]
        [return: ServiceBus("order-created", Connection = "ServiceBus", EntityType = EntityType.Topic)]
        public static async Task<Message> HandleCreateOrder([ServiceBusTrigger("create-order", Connection = "ServiceBus")] Message message)
        {
            var command = JsonConvert.DeserializeObject<CreateOrderCommand>(Encoding.UTF8.GetString(message.Body));

            // Simulate a hard work
            await Task.Delay(new Random().Next(250, 1000));

            var @event = new OrderCreatedEvent
            {
                Id = command.Id
            };
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            return new Message(bytes)
            {
                ReplyTo = message.ReplyTo,
                CorrelationId = message.MessageId,
                SessionId = message.SessionId
            };
        }
    }
}
