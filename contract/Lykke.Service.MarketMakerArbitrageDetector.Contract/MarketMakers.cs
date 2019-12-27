using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Contract
{
    /// <summary>
    /// Represents a list of market makers that are in arbitrages.
    /// </summary>
    public class MarketMakers
    {
        public IReadOnlyList<string> Names { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
