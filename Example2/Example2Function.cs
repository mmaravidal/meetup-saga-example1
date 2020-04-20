using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Example2
{
    public static class Example2Function
    {
        [FunctionName("Example2-Client")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            using var reader = new StreamReader(request.Body);
            using var json = new JsonTextReader(reader);

            var command = JsonSerializer.CreateDefault().Deserialize<CreateOrderRequest>(json);

            var id = await starter.StartNewAsync("Example2-Orchestrator", command);

            return new CreatedResult("", new CreateOrderResponse
            {
                Id = id,
                Name = command.Name
            });
        }

        [FunctionName("Example2-Orchestrator")]
        public static async Task StartAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var command = context.GetInput<CreateOrderRequest>();
            
            // Create the order
            await context.CallActivityAsync("Example2-CreateOrder", new CreateOrderCommand
            {
                Id = context.InstanceId,
                Name = command.Name
            });

            try
            {
                foreach (var item in command.Items)
                {
                    // Reserve the item stock
                    await context.CallActivityAsync("Example2-LeaseStock", new LeaseStockCommand
                    {
                        Item = item.Key,
                        Quantity = item.Value
                    });
                }
            }
            catch
            {
                // Compensating a failure. Removing the reservation.
                foreach (var item in command.Items)
                {
                    await context.CallActivityAsync("Example2-UnleaseStock", new UnleaseStockCommand
                    {
                        Item = item.Key,
                        Quantity = item.Value
                    });
                }
                
            }
        }

        [FunctionName("Example2-CreateOrder")]
        public static async Task CreateOrder([ActivityTrigger] CreateOrderCommand command, ILogger logger)
        {
            logger.LogWarning("Creating order");

            // Simulate a hard work ordering the item
            await Task.Delay(100);
        }

        [FunctionName("Example2-LeaseStock")]
        public static async Task LeaseStock([ActivityTrigger] LeaseStockCommand command, ILogger logger)
        {
            logger.LogWarning("Ordering item");

            // Simulate a hard work ordering the item
            await Task.Delay(100);

            // Oh wait! If there was no stock, throw.
            if (string.Equals(command.Item, "inexistent", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogWarning("There was no stock aivalable!");

                throw new InvalidOperationException("There is no stock of product 'unexistent'");
            }
        }

        [FunctionName("Example2-UnleaseStock")]
        public static async Task UnleaseStock([ActivityTrigger] UnleaseStockCommand command, ILogger logger)
        {
            logger.LogWarning("Unleasing stock");

            // Simulate a hard work ordering the item
            await Task.Delay(100);
        }
    }
}
