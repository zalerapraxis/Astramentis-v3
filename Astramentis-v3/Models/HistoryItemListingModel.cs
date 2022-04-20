using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Models
{
    public class HistoryItemListingModel
    {
        public string Name { get; set; }
        public int ItemId { get; set; }
        public int SoldPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsHq { get; set; }
        public DateTime SaleDate { get; set; }
        public string Server { get; set; }
    }
}
