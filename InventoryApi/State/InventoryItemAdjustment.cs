using System;

namespace InventoryApi.State
{
    public class InventoryItemAdjustment
    {
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public DateTime AdjustedOn { get; set; }
        public string Action { get; set; }
    }
}
