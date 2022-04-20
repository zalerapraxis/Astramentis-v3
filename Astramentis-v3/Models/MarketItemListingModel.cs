using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Models
{
    public class MarketItemListingModel
    {
        public string Name { get; set; }
        public int ItemId { get; set; }
        public int CurrentPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsHq { get; set; }
        public string RetainerName { get; set; }
        public string Server { get; set; }
    }
}
