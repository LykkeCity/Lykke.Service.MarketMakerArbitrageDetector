using System.Threading.Tasks;
using Common;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public ShutdownManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber, IArbitrageDetectorService arbitrageDetectorService)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        public Task StopAsync()
        {
            _lykkeOrderBookSubscriber.Stop();
            ((IStopable)_arbitrageDetectorService).Stop();

            return Task.CompletedTask;
        }
    }
}
