using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Service.MarketMakerArbitrageDetector.Rabbit.Models
{
    public class LykkeOrderBook
    {
        [JsonProperty("assetPair")]
        public string AssetPairId { get; set; }

        public bool IsBuy { get; set; }

        public DateTime Timestamp { get; set; }

        [JsonProperty("prices")]
        public IReadOnlyList<LykkeLimitOrder> LimitOrders { get; set; }
    }
}
