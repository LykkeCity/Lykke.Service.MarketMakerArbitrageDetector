using System.Threading.Tasks;
using Common;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;
using Lykke.Service.MarketMakerArbitrageDetector.Services.Publishers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly IMarketMakersPublisher _marketMakersPublisher;

        public ShutdownManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber,
            IArbitrageDetectorService arbitrageDetectorService,
            IMarketMakersPublisher marketMakersPublisher)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
            _marketMakersPublisher = marketMakersPublisher;
        }

        public Task StopAsync()
        {
            _lykkeOrderBookSubscriber.Stop();
            _marketMakersPublisher.Stop();
            ((IStopable)_arbitrageDetectorService).Stop();

            return Task.CompletedTask;
        }
    }
}
