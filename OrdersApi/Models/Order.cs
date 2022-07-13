using System;
using System.Collections.Generic;

namespace OrdersApi.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string CustomerCode { get; set; }
        public DateTime OrderCreatedOn { get; set; }
        public string OrderStatus { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
