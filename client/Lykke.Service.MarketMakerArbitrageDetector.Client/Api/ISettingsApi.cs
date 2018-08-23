using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface ISettingsApi
    {
        [Get("/api/settings")]
        Task<Settings> GetAsync();

        [Post("/api/settings")]
        Task SetAsync(Settings settings);
    }
}
