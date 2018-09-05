using System.Threading.Tasks;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    public class StartupManager : IStartupManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public StartupManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber, IArbitrageDetectorService arbitrageDetectorService)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        public Task StartAsync()
        {
            _lykkeOrderBookSubscriber.Start();
            ((IStartable)_arbitrageDetectorService).Start();

            return Task.CompletedTask;
        }
    }
}
