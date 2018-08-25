using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IArbitragesApi
    {
        [Get("/api/arbitrages")]
        Task<IReadOnlyCollection<Arbitrage>> GetAllAsync(string target, string source);
    }
}
