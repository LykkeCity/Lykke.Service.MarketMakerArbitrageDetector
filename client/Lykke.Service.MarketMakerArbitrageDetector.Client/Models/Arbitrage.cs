using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class Arbitrage
    {
        public AssetPair Target { get; set; }

        public AssetPair Source { get; set; }

        public int SourcesCount { get; set; }

        public int SynthsCount { get; set; }

        public decimal Spread { get; set; }

        public string TargetSide { get; set; }

        public string ConversionPath { get; set; }

        public decimal Volume { get; set; }

        public decimal? VolumeInUsd { get; set; }

        public decimal PnL { get; set; }

        public decimal? PnLInUsd { get; set; }

        public decimal? TargetAsk { get; set; }

        public decimal? TargetBid { get; set; }

        public decimal? SynthAsk { get; set; }

        public decimal? SynthBid { get; set; }

        public IReadOnlyList<string> MarketMakers { get; set; } = new List<string>();

        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
