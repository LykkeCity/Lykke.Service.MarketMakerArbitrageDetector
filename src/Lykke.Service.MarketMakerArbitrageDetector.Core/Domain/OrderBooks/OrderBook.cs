using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; }

        public AssetPair AssetPair { get; private set; }

        public IReadOnlyList<LimitOrder> Bids { get; }

        public IReadOnlyList<LimitOrder> Asks { get; }

        public DateTime Timestamp { get; }

        public OrderBook(string exchange, AssetPair assetPair, IReadOnlyList<LimitOrder> bids, IReadOnlyList<LimitOrder> asks, DateTime timestamp)
        {
            Exchange = !string.IsNullOrWhiteSpace(exchange) ? exchange : throw new ArgumentException($"Argument '{nameof(exchange)}' is null or empty.");
            AssetPair = assetPair != null ? assetPair : throw new ArgumentNullException($"Argument '{nameof(assetPair)}' is null.");
            Bids = bids;
            Asks = asks;
            Timestamp = timestamp;
        }

        public void SetAssetPair(AssetPair assetPair)
        {
            AssetPair = assetPair;
        }

        public override string ToString()
        {
            return $"{Exchange} - {AssetPair}, Bids: {Bids.Count}, Asks: {Asks.Count}, Timestamp: {Timestamp}";
        }
    }
}
