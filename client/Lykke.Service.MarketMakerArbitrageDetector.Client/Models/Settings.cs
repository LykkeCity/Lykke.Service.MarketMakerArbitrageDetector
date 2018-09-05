using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Settings for market maker arbitrage detector.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// List of wallets where the key is wallet identifier and value is wallet display name.
        /// </summary>
        public Dictionary<string, string> Wallets { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Interval between two attempts to find arbitrages.
        /// </summary>
        public TimeSpan ExecutionInterval { get; set; } = new TimeSpan(0, 0, 0, 2);
    }
}
