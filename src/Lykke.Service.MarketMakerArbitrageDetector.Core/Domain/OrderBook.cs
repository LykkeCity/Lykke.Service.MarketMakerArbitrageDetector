using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class OrderBook
    {
        public string Exchange { get; }

        public AssetPair AssetPair { get; private set; }

        public IReadOnlyCollection<LimitOrder> Bids { get; }

        public IReadOnlyCollection<LimitOrder> Asks { get; }

        public LimitOrder BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : null;

        public LimitOrder BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : null;

        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        public decimal AsksVolume => Asks.Sum(x => x.Volume);

        public DateTime Timestamp { get; }


        public OrderBook(string exchange, AssetPair assetPair, IReadOnlyCollection<LimitOrder> bids, IReadOnlyCollection<LimitOrder> asks, DateTime timestamp)
        {
            Exchange = exchange;
            AssetPair = assetPair;
            Bids = bids;
            Asks = asks;
            Timestamp = timestamp;
        }

        public void SetAssetPair(AssetPair assetPair)
        {
            AssetPair = assetPair;
        }

        public OrderBook Reverse()
        {
            var result = new OrderBook (
                Exchange,
                AssetPair.Reverse(),
                Bids.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Asks.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Timestamp
            );

            return result;
        }

        public override string ToString()
        {
            return $"{Exchange} - {AssetPair}, Bids: {Bids.Count}, Asks: {Asks.Count}, BestBid: {BestBid?.Price}, BestAsk: {BestAsk?.Price}, Timestamp: {Timestamp}";
        }
    }
}
