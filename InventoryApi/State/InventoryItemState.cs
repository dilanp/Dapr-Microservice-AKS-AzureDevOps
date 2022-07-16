using System.Collections.Generic;

namespace InventoryApi.State
{
    public class InventoryItemState
    {
        public string SKU { get; set; }
        public int Balance { get; set; }
        public List<InventoryItemAdjustment> Changes { get; set; }
    }
}
