using System;
using System.Collections.Generic;

namespace InventoryApi.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string CustomerCode { get; set; }
        public DateTime OrderCreatedOn { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
