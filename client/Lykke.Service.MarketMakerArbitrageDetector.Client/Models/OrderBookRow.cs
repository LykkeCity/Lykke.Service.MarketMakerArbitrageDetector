using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class OrderBookRow
    {
        public string Exchange { get; set; }

        public string AssetPair { get; set; }

        public IReadOnlyCollection<string> MarketMakers { get; set; } = new List<string>();

        public decimal? BestBid { get; set; }

        public decimal? BestAsk { get; set; }

        public decimal BidsVolume { get; set; }

        public decimal? BidsVolumeInUsd { get; set; }

        public decimal AsksVolume { get; set; }

        public decimal? AsksVolumeInUsd { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Exchange} - {AssetPair}";
        }
    }
}
