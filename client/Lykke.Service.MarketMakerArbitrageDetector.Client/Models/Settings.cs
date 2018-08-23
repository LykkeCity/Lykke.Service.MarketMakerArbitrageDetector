using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class Settings
    {
        public Dictionary<string, string> Wallets { get; set; } = new Dictionary<string, string>();

        public TimeSpan ExecutionInterval { get; set; } = new TimeSpan(0, 0, 0, 2);
    }
}
