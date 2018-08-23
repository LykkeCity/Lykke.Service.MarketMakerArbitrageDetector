using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IArbitragesApi
    {
        [Get("/api/arbitrages")]
        IReadOnlyCollection<Arbitrage> GetAll();
    }
}
