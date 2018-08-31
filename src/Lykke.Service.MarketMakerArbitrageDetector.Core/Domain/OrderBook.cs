using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
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


        public OrderBook(string exchange, AssetPair assetPair, IEnumerable<LimitOrder> bids, IEnumerable<LimitOrder> asks, DateTime timestamp)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(exchange));
            Debug.Assert(assetPair != null);

            Exchange = exchange;
            AssetPair = assetPair;
            Bids = bids?.OrderByDescending(x => x.Price).ToList();
            Asks = asks?.OrderBy(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        public void SetAssetPair(AssetPair assetPair)
        {
            AssetPair = assetPair;
        }

        public OrderBook Invert()
        {
            var newBids = Asks.Select(x => x.Reciprocal()).ToList();
            var newAsks = Bids.Select(x => x.Reciprocal()).ToList();

            var result = new OrderBook(Exchange, AssetPair.Invert(), newBids, newAsks, Timestamp);

            return result;
        }

        public override string ToString()
        {
            return $"{Exchange} - {AssetPair}, Bids: {Bids.Count()}, Asks: {Asks.Count()}, BestBid: {BestBid?.Price}, BestAsk: {BestAsk?.Price}, Timestamp: {Timestamp}";
        }
    }
}
