using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryDash.Models
{
    public class DailyInventory
    {
        public int ID { get; set; }
        public string MenuItem { get; set; }
        public int Quantity { get; set; }
    }
}