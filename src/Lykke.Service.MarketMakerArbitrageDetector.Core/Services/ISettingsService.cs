using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.Settings;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface ISettingsService
    {
        Task<Settings> GetAsync();

        Task SetAsync(Settings settings);
    }
}
