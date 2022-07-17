using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using InventoryApi.Models;
using InventoryApi.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedLib;

namespace InventoryApi.Controllers
{
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private const string PubSub = "messagebus"; // The name specified in pubsub.yaml file.
        private const string InventoryStoreName = "inventorystore"; // The name specified in inventorystore-redis.yaml
        private const string InventoryItemStoreName = "inventoryitemstore"; // The name specified in inventoryitemstore-redis.yaml

        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ILogger<InventoryController> logger)
        {
            _logger = logger;
        }

        // Subscribes to the topic - OrderCreatedTopicName
        [HttpPost(CommonPubSubTopics.OrderCreatedTopicName)]
        [Topic(PubSub, CommonPubSubTopics.OrderCreatedTopicName)]
        public async Task<ActionResult> InventoryAdjustment(Order order, [FromServices] DaprClient daprClient)
        {
            // Check if this order already exists in the state store
            var inventoryState = await daprClient
                .GetStateEntryAsync<InventoryState>(InventoryStoreName, order.OrderId.ToString());
            // If an order doesn't exists then, set this order.
            inventoryState.Value ??= new InventoryState
            {
                OrderId = order.OrderId,
                Items = order.Items,
                CustomerCode = order.CustomerCode,
                OrderCreatedOn = order.OrderCreatedOn
            };

            // Iterate over the order items and get their code and quantity.
            foreach (var item in order.Items.ToList())
            {
                var sku = item.ItemCode;
                var quantity = item.Quantity;

                // Get current inventory item state.
                var itemState = await daprClient.GetStateEntryAsync<InventoryItemState>(InventoryItemStoreName, sku);
                
                // If there's no inventory then initiate with a balance of 100!
                itemState.Value ??= new InventoryItemState { SKU = sku, Balance = 100, Changes = new List<InventoryItemAdjustment>() };
                
                // Update the balance.
                itemState.Value.Balance -= quantity;
                
                // Add an accompanying inventory history record.
                itemState.Value.Changes.Add(new InventoryItemAdjustment
                {
                    SKU = sku,
                    Quantity = quantity,
                    Action = "Sold",
                    AdjustedOn = DateTime.UtcNow
                });
                
                // We only record history of last 5 changes!
                if (itemState.Value.Changes.Count > 5)
                    itemState.Value.Changes.RemoveAt(0);
                
                // Save the changes into the item state.
                await itemState.SaveAsync();
                // Log.
                Console.WriteLine($"Reservation in {order.OrderId} of {sku} for {quantity}, balance {itemState.Value.Balance}");
            }

            // Save inventory back to state store.
            await inventoryState.SaveAsync();

            // Publish to the order prepared topic.
            await daprClient.PublishEventAsync(PubSub, CommonPubSubTopics.OrderPreparedTopicName, order);

            // Log
            Console.WriteLine($"Preparation for order {order.OrderId} completed.");

            return Ok();
        }

        // GET the inventory balance.
        [HttpGet("balance/{state}")]
        public ActionResult<InventoryItemState> Get(
            [FromState(InventoryItemStoreName)] StateEntry<InventoryItemState> state, 
            [FromServices] DaprClient daprClient)
        {
            Console.WriteLine("Entered Item State retrieval");
            if (state.Value == null)
                return NotFound();

            var result = new InventoryItemState
            {
                SKU = state.Value.SKU,
                Balance = state.Value.Balance,
                Changes = state.Value.Changes
            };

            Console.WriteLine($"Retrieved {result.SKU} is {result.Balance}.");
            return result;
        }

        [HttpPost("/stockRefill")]
        public async Task<ActionResult> Refill([FromServices] DaprClient daprClient)
        {
            using (var reader = new System.IO.StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var item = JsonSerializer.Deserialize<dynamic>(body);
                var sku = item.GetProperty("SKU").GetString();
                var quantity = item.GetProperty("Quantity").GetInt32();
                var itemState = await daprClient.GetStateEntryAsync<InventoryItemState>(InventoryItemStoreName, sku);
                itemState.Value ??= new InventoryItemState
                {
                    SKU = sku,
                    Changes = new List<InventoryItemAdjustment>()
                };
                itemState.Value.Changes.Add(new InventoryItemAdjustment
                {
                    SKU = sku,
                    Action = "Refill",
                    AdjustedOn = DateTime.UtcNow,
                    Quantity = quantity
                });

                // Update the balance now.
                itemState.Value.Balance += quantity;

                //Save
                await itemState.SaveAsync();
                Console.WriteLine($"Refill of {sku} for quantity {quantity}, new balance {itemState.Value.Balance}");
            }

            return Ok();
        }
    }
}
