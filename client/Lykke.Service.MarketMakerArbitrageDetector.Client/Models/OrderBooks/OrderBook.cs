using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; set; }

        public AssetPair AssetPair { get; set; }

        public IReadOnlyList<LimitOrder> Bids { get; set; }

        public IReadOnlyList<LimitOrder> Asks { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Exchange} - {AssetPair}, Bids: {Bids.Count}, Asks: {Asks.Count}, Timestamp: {Timestamp}";
        }
    }
}
