using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; }

        public AssetPair AssetPair { get; private set; }

        public IReadOnlyList<LimitOrder> Bids { get; }

        public IReadOnlyList<LimitOrder> Asks { get; }

        public LimitOrder BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : null;

        public LimitOrder BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : null;

        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        public decimal AsksVolume => Asks.Sum(x => x.Volume);

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
            return $"{Exchange} - {AssetPair}, BestBid: {BestBid:0.#####}, BestAsk: {BestAsk:0.#####}, Bids: {Bids.Count}, Asks: {Asks.Count}, Timestamp: {Timestamp}";
        }
    }
}
