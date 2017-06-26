using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryDash.Models
{
    public class WeeklyInventoryMain
    {
        public int ID { get; set; }
        public int WeekOfYear { get; set; }
        public int Year { get; set; }
    }
}