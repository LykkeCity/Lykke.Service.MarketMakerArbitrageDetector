using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.MarketMakerArbitrageDetector.Services.Publishers;
using Lykke.Service.MarketMakerArbitrageDetector.Settings;

namespace Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Publishers
{
    [UsedImplicitly]
    public class MarketMakersPublisher : IMarketMakersPublisher
    {
        private readonly ILogFactory _logFactory;
        private readonly MainRabbitMqSettings _settings;
        private RabbitMqPublisher<Contract.MarketMakers> _publisher;
        private readonly ILog _log;

        public MarketMakersPublisher(MainRabbitMqSettings settings, ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Publish(Contract.MarketMakers marketMakers)
        {
            _publisher.ProduceAsync(marketMakers);

            _log.Info($"Published market makers: {marketMakers.ToJson()}.");
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.MarketMakersExchange);

            _publisher = new RabbitMqPublisher<Contract.MarketMakers>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<Contract.MarketMakers>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
