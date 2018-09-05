using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents summary information of an order book.
    /// </summary>
    public class OrderBookRow
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
        /// Best bid (the biggest one).
        /// </summary>
        public decimal? BestBid { get; set; }

        /// <summary>
        /// Best ask (the smallest one).
        /// </summary>
        public decimal? BestAsk { get; set; }

        /// <summary>
        /// Total volume of all bids.
        /// </summary>
        public decimal BidsVolume { get; set; }

        /// <summary>
        /// Total volume of all bids converted to $.
        /// </summary>
        public decimal? BidsVolumeInUsd { get; set; }

        /// <summary>
        /// Total volume of all asks.
        /// </summary>
        public decimal AsksVolume { get; set; }

        /// <summary>
        /// Total volume of all asks converted to $.
        /// </summary>
        public decimal? AsksVolumeInUsd { get; set; }

        /// <summary>
        /// Timestamp of the current state.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// List of merket makers in this order book.
        /// </summary>
        public IReadOnlyList<string> MarketMakers { get; set; } = new List<string>();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source} - {AssetPair}";
        }
    }
}
