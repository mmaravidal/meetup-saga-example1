using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using Example2.Events.Models;

namespace Example2.Events
{
    public static class Example2EventsFunction
    {
        [FunctionName("Example2-Events-Client")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            using var reader = new StreamReader(request.Body);
            using var json = new JsonTextReader(reader);

            var command = JsonSerializer.CreateDefault().Deserialize<CreateOrderRequest>(json);

            var id = await starter.StartNewAsync("Example2-Events-Orchestrator", command);

            return new AcceptedResult("", new CreateOrderResponse
            {
                Id = id,
                Name = command.Name
            });
        }

        [FunctionName("Example2-Events-Orchestrator")]
        public static async Task StartAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var request = context.GetInput<CreateOrderRequest>();

            await CreateOrder(context, new CreateOrderCommand
            {
                Id = context.InstanceId,
                Name = request.Name
            });

            var leased = new List<StockLeasedEvent>();

            try
            {
                foreach (var item in request.Items)
                {
                    leased.Add(await LeaseStock(context, new LeaseStockCommand
                    {
                        Id = request.Id,
                        Item = item.Key,
                        Quantity = item.Value
                    }));
                }
            }
            catch (CompensationEventDetectedException)
            {
                foreach(var @event in leased)
                {
                    await UnleaseStock(context, new UnleaseStockCommand
                    {
                        Id = request.Id,
                        Item = @event.Item,
                        Quantity = @event.Quantity
                    });
                }
            }
        }

        private static async Task<OrderCreatedEvent> CreateOrder(IDurableOrchestrationContext context, CreateOrderCommand command)
        {
            // Start Activity CreateOrder
            await context.CallActivityAsync("Example2-Events-CreateOrder", command);

            // Wait for completion
            return await context.WaitForExternalEvent<OrderCreatedEvent>($"order-created");
        }

        private static async Task<StockLeasedEvent> LeaseStock(IDurableOrchestrationContext context, LeaseStockCommand command)
        {
            // Start LeaseStock activity
            await context.CallActivityAsync("Example2-Events-LeaseStock", command);

            // Wait for completion
            var stockLeasedTask = context.WaitForExternalEvent<StockLeasedEvent>($"stock-leased");
            var stockLeaseDeniedTask = context.WaitForExternalEvent<StockLeaseDeniedEvent>($"stock-lease-denied");

            var completedTask = await Task.WhenAny(
                stockLeasedTask,
                stockLeaseDeniedTask);

            if (completedTask == stockLeaseDeniedTask)
            {
                throw new CompensationEventDetectedException(stockLeaseDeniedTask.Result);
            }

            return stockLeasedTask.Result;
        }

        private static async Task<StockUnleasedEvent> UnleaseStock(IDurableOrchestrationContext context, UnleaseStockCommand command)
        {
            // Start Activity CreateOrder
            await context.CallActivityAsync("Example2-Events-UnleaseStock", command);

            // Wait for completion
            return await context.WaitForExternalEvent<StockUnleasedEvent>($"stock-unleased");
        }
    }
}
