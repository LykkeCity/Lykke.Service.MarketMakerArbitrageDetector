using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface ISettingsApi
    {
        /// <summary>
        /// Returns settings of market maker arbitrage detector.
        /// </summary>
        [Get("/api/settings")]
        Task<Settings> GetAsync();

        /// <summary>
        /// Applies new settings to market maker arbitrage detector.
        /// </summary>
        [Post("/api/settings")]
        Task SetAsync(Settings settings);
    }
}
