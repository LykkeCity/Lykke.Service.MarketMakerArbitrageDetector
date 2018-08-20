using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Rabbit.Subscribers;

namespace Lykke.Service.MarketMakerArbitrageDetector.Managers
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;

        public StartupManager(LykkeOrderBookSubscriber lykkeOrderBookSubscriber)
        {
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
        }

        public Task StartAsync()
        {
            _lykkeOrderBookSubscriber.Start();

            return Task.CompletedTask;
        }
    }
}
