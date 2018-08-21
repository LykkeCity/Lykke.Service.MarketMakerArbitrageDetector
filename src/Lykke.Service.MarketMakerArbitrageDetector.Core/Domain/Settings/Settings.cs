using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.Settings
{
    public class Settings
    {
        public Dictionary<string, string> Wallets = new Dictionary<string, string>();

        public TimeSpan ExecutionInterval = new TimeSpan(0, 0, 0, 2);
    }
}
