using Example2.Events.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Example2.Events.Fakes
{
    public class StockService
    {
        [FunctionName("Example2-Events-FakeStockService-LeaseStockHandler")]
        public static async Task HandleLeaseStock(
            [ServiceBusTrigger("lease-stock", Connection = "ServiceBus")] Message message,
            [ServiceBus("stock-leased", Connection = "ServiceBus", EntityType = EntityType.Topic)] ICollector<Message> stockLeasedMessage,
            [ServiceBus("stock-lease-denied", Connection = "ServiceBus", EntityType = EntityType.Topic)] ICollector<Message> stockLeaseDeniedMessage,
            ILogger logger)
        {
            logger.LogWarning("FakeStockService: Leasing stock");
            var command = JsonConvert.DeserializeObject<LeaseStockCommand>(Encoding.UTF8.GetString(message.Body));

            // Simulate a hard work
            logger.LogWarning("FakeStockService: Working...");
            await Task.Delay(new Random().Next(250, 1000));

            if (command.Item == "inexistent")
            {
                var @event = new StockLeaseDeniedEvent
                {
                    Id = command.Id,
                    Item = command.Item,
                    Quantity = command.Quantity
                };

                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

                logger.LogWarning("FakeStockService: Stock lease denied");
                stockLeaseDeniedMessage.Add(new Message(bytes)
                {
                    ReplyTo = message.ReplyTo,
                    CorrelationId = message.MessageId,
                    SessionId = message.SessionId
                }); 
            }
            else
            {
                var @event = new StockLeasedEvent
                {
                    Id = command.Id,
                    Item = command.Item,
                    Quantity = command.Quantity
                };

                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

                logger.LogWarning("FakeStockService: Stock leased");
                stockLeasedMessage.Add(new Message(bytes)
                {
                    ReplyTo = message.ReplyTo,
                    CorrelationId = message.MessageId,
                    SessionId = message.SessionId
                });
            }
        }

        [FunctionName("Example2-Events-FakeStockService-UnleaseStockHandler")]
        [return: ServiceBus("stock-unleased", Connection = "ServiceBus", EntityType = EntityType.Topic)]
        public static async Task<Message> HandleUnleaseStock(
            [ServiceBusTrigger("unlease-stock", Connection = "ServiceBus")] Message message,
            ILogger logger)
        {
            logger.LogWarning("FakeStockService: Unleasing stock");
            var command = JsonConvert.DeserializeObject<UnleaseStockCommand>(Encoding.UTF8.GetString(message.Body));

            // Simulate a hard work
            logger.LogWarning("FakeStockService: Working...");
            await Task.Delay(new Random().Next(250, 1000));

            var @event = new StockUnleasedEvent
            {
                Id = command.Id,
                Item = command.Item,
                Quantity = command.Quantity
            };
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            logger.LogWarning("FakeStockService: Stock unleased");
            return new Message(bytes)
            {
                ReplyTo = message.ReplyTo,
                CorrelationId = message.MessageId,
                SessionId = message.SessionId
            };
        }
    }
}
