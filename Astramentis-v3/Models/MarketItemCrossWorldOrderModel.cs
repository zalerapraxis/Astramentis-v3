using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Models
{
    // used for both supplying the xworder func with params and returning the results back to the calling command
    public class MarketItemCrossWorldOrderModel
    {
        public string Name { get; set; } // input value
        public int ItemID { get; set; } // input value
        public int Price { get; set; } // input value
        public int Quantity { get; set; } // input value

        public bool IsHQ { get; set; } // return value
        public bool ShouldBeHQ { get; set; } // parameter value
        public int NeededQuantity { get; set; } // parameter value
        public string Server { get; set; } // return value
    }

    public class MarktetItemXWOrderListModel
    {
        public List<MarketItemCrossWorldOrderModel> List { get; set; }
        public int TotalCost { get; set; }
    }
}
