using System.Threading.Tasks;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;
using Lykke.Service.MarketMakerArbitrageDetector.Services.Publishers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    public class StartupManager : IStartupManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly IMarketMakersPublisher _marketMakersPublisher;

        public StartupManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber,
            IArbitrageDetectorService arbitrageDetectorService,
            IMarketMakersPublisher marketMakersPublisher)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
            _marketMakersPublisher = marketMakersPublisher;
        }

        public Task StartAsync()
        {
            _lykkeOrderBookSubscriber.Start();
            _marketMakersPublisher.Start();
            ((IStartable)_arbitrageDetectorService).Start();

            return Task.CompletedTask;
        }
    }
}
