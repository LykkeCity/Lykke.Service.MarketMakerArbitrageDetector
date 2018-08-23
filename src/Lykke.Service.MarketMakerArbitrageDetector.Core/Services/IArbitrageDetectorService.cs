using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        IReadOnlyCollection<Arbitrage> GetAll();
    }
}
