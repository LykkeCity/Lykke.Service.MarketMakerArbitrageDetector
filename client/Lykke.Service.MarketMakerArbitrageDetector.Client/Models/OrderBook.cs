using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an order book.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Source of the order book.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset pair of the order book.
        /// </summary>
        public AssetPair AssetPair { get; set; }

        /// <summary>
        /// Bids or buy orders of the order book.
        /// </summary>
        public IReadOnlyList<LimitOrder> Bids { get; set; }

        /// <summary>
        /// Asks or sell orders of the order book.
        /// </summary>
        public IReadOnlyList<LimitOrder> Asks { get; set; }

        /// <summary>
        /// Best bid (the biggest one).
        /// </summary>
        public LimitOrder BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : null;

        /// <summary>
        /// Best ask (the smallest one).
        /// </summary>
        public LimitOrder BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : null;

        /// <summary>
        /// Total volume of all bids.
        /// </summary>
        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        /// <summary>
        /// Total volume of all asks.
        /// </summary>
        public decimal AsksVolume => Asks.Sum(x => x.Volume);

        /// <summary>
        /// Timestamp of the current state.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source} - {AssetPair}, Bids: {Bids.Count}, Asks: {Asks.Count}, BestBid: {BestBid?.Price:0.#####}, BestAsk: {BestAsk?.Price:0.#####}, Timestamp: {Timestamp}";
        }
    }
}
