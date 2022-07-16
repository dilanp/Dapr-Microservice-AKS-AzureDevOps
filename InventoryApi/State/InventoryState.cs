using System;
using System.Collections.Generic;
using InventoryApi.Models;

namespace InventoryApi.State
{
    public class InventoryState
    {
        public Guid OrderId { get; set; }
        public string CustomerCode { get; set; }
        public DateTime OrderCreatedOn { get; set; }
        public List<OrderItem>? Items { get; set; }
    }
}
