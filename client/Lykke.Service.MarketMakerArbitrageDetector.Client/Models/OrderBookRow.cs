using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class OrderBookRow
    {
        public string Source { get; set; }

        public AssetPair AssetPair { get; set; }

        public decimal? BestBid { get; set; }

        public decimal? BestAsk { get; set; }

        public decimal BidsVolume { get; set; }

        public decimal? BidsVolumeInUsd { get; set; }

        public decimal AsksVolume { get; set; }

        public decimal? AsksVolumeInUsd { get; set; }

        public DateTime Timestamp { get; set; }

        public IReadOnlyList<string> MarketMakers { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{Source} - {AssetPair}";
        }
    }
}
