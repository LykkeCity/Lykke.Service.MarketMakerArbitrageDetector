using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    [UsedImplicitly]
    public class ShutdownManager : IShutdownManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;

        public ShutdownManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
        }

        public Task StopAsync()
        {
            _lykkeOrderBookSubscriber.Stop();

            return Task.CompletedTask;
        }
    }
}
