using System;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrdersApi.Models;
using SharedLib;

namespace OrdersApi.Controllers
{
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private const string PubSub = "messagebus";
        private const string StoreName = "orderstore";

        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto, [FromServices] DaprClient daprClient)
        {
            if (!Validate(orderDto))
                return BadRequest();

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderStatus = "Created",
                CustomerCode = orderDto.CustomerCode,
                OrderCreatedOn = DateTime.UtcNow,
                Items = orderDto.Items
            };

            // Save order to state store using OrderOd as the key.
            // Use DaprClient and its SaveState() method.
            await daprClient.SaveStateAsync(StoreName, order.OrderId.ToString(), order);

            // Publish the order to the order created message queue.
            await daprClient.PublishEventAsync(PubSub, CommonPubSubTopics.OrderCreatedTopicName, order);

            return Ok(order.OrderId);
        }

        [HttpGet("order/{state}")]
        public ActionResult<Order> Get([FromState(StoreName)] StateEntry<Order> state)
        {
            if (state.Value == null)
                return NotFound();

            var result = state.Value;

            Console.WriteLine($"Retrieved order {result.OrderId}");

            return result;
        }

        private static bool Validate(OrderDto orderDto)
        {
            var summedItem =
                from item in orderDto.Items
                group item by item.ItemCode into items
                select new
                {
                    ProductCode = items.Key,
                    Quantity = items.Sum(x => x.Quantity)
                };
            return true;
        }
    }
}
