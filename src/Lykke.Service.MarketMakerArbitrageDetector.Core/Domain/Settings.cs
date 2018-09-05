using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class Settings
    {
        public Dictionary<string, string> Wallets { get; set; } = new Dictionary<string, string>();

        public TimeSpan ExecutionInterval { get; set; } = new TimeSpan(0, 0, 0, 2);
    }
}
