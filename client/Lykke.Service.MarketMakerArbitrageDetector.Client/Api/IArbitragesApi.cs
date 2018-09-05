using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IArbitragesApi
    {
        /// <summary>
        /// Returns all arbitrages from the last attempt.
        /// </summary>
        [Get("/api/arbitrages")]
        Task<IReadOnlyCollection<Arbitrage>> GetAllAsync(string target, string source);
    }
}
