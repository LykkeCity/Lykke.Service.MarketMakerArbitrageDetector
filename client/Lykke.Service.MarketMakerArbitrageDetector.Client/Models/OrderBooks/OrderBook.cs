using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; set; }

        public AssetPair AssetPair { get; set; }

        public IReadOnlyList<LimitOrder> SellLimitOrders { get; set; }

        public IReadOnlyList<LimitOrder> BuyLimitOrders { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
