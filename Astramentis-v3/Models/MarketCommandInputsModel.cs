using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Models
{
    public class MarketCommandInputsModel
    {
        public string ItemName { get; set; }
        public int ItemID { get; set; }
        public string ItemIconUrl { get; set; }
        public bool ItemHq { get; set; }
        public int NeededQuantity { get; set; } // for market order command
        public List<string> WorldsToSearch { get; set; }
    }
}
