﻿namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class AssetPair
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Asset Base { get; set; }

        public Asset Quote { get; set; }

        public override string ToString()
        {
            return $"{Base.Name}/{Quote.Name}";
        }
    }
}