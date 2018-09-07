using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage oportunity between two order books - with target and source asset pairs.
    /// Order book with source asset pair usually synthetic i.e. compiled of 2-3 original order books.
    /// </summary>
    public class Arbitrage
    {
        /// <summary>
        /// Asset pair of targer order book.
        /// </summary>
        public AssetPair Target { get; set; }

        /// <summary>
        /// Asset pair of source order book.
        /// </summary>
        public AssetPair Source { get; set; }

        /// <summary>
        /// Count of arbitrages with current source asset pair after grouping by target asset pair.
        /// </summary>
        public int SourcesCount { get; set; }

        /// <summary>
        /// Count of arbitrages with current source and target asset pairs after grouping by target asset pair.
        /// </summary>
        public int SynthsCount { get; set; }

        /// <summary>
        /// Spread of arbitrage between order books.
        /// Always negative.
        /// Best one if was grouped.
        /// </summary>
        public decimal Spread { get; set; }

        /// <summary>
        /// Side of the target order book which participate in arbitrage.
        /// </summary>
        public string TargetSide { get; set; }

        /// <summary>
        /// Conversion path of a synthetic order book.
        /// </summary>
        public string ConversionPath { get; set; }

        /// <summary>
        /// Volume of arbitrage after trading simulation, in base asset of target asset pair.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Volume converted to $ with corresponding order book.
        /// </summary>
        public decimal? VolumeInUsd { get; set; }

        /// <summary>
        /// PnL of arbitrage after trading simulation, in quote asset of target asset pair.
        /// </summary>
        public decimal PnL { get; set; }

        /// <summary>
        /// PnL converted to $ with corresponding order book.
        /// </summary>
        public decimal? PnLInUsd { get; set; }

        /// <summary>
        /// Best ask of the target order book.
        /// </summary>
        public decimal? TargetAsk { get; set; }

        /// <summary>
        /// Best bid of the target order book.
        /// </summary>
        public decimal? TargetBid { get; set; }

        /// <summary>
        /// Best ask of the synthetic order book.
        /// </summary>
        public decimal? SynthAsk { get; set; }

        /// <summary>
        /// Best bid of the synthetic order book.
        /// </summary>
        public decimal? SynthBid { get; set; }

        /// <summary>
        /// List of merket makers in this arbitrage.
        /// </summary>
        public IReadOnlyList<string> MarketMakers { get; set; } = new List<string>();

        /// <summary>
        /// Timestamp of an arbitrage creation, in UTC.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
