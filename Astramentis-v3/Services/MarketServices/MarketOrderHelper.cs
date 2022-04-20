using Astramentis.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Services.MarketServices
{
    public class MarketOrderHelper
    {
        public int rngId = 0;

        public List<List<MarketItemCrossWorldOrderModel>> ValidResults = new List<List<MarketItemCrossWorldOrderModel>>();

        public MarketOrderHelper()
        {
        }

        public List<List<MarketItemCrossWorldOrderModel>> SumUp(List<MarketItemCrossWorldOrderModel> listings, int target)
        {            
            sum_up_recursive(listings, target, new List<MarketItemCrossWorldOrderModel>());
            
            return ValidResults;
        }

        private void sum_up_recursive(List<MarketItemCrossWorldOrderModel> listings, int target, List<MarketItemCrossWorldOrderModel> partialListings)
        {
            int sum = 0;
            foreach (var x in partialListings)
            {
                sum += x.Quantity;
            }

            if (sum >= target)
            {
                ValidResults.Add(partialListings);
                return;
            }

            for (int i = 0; i < listings.Count; i++)
            {
                List<MarketItemCrossWorldOrderModel> remaining = new List<MarketItemCrossWorldOrderModel>();
                MarketItemCrossWorldOrderModel n = listings[i];
                for (int j = i + 1; j < listings.Count; j++) remaining.Add(listings[j]);

                List<MarketItemCrossWorldOrderModel> partial_rec = new List<MarketItemCrossWorldOrderModel>(partialListings);
                partial_rec.Add(n);
                sum_up_recursive(remaining, target, partial_rec);
            }
        }
    }
}
