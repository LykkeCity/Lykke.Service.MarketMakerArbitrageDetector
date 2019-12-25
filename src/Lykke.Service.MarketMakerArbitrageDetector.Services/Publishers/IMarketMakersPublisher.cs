using Lykke.Service.MarketMakerArbitrageDetector.Contract;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services.Publishers
{
    public interface IMarketMakersPublisher
    {
        void Publish(MarketMakers marketMakers);
    }
}
