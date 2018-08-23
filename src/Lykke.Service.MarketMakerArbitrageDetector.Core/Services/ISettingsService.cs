using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface ISettingsService
    {
        Settings Get();

        void Set(Settings settings);
    }
}
