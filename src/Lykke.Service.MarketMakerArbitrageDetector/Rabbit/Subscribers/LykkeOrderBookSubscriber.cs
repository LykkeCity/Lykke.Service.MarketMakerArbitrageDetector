using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Handlers;
using Lykke.Service.MarketMakerArbitrageDetector.Rabbit.Models;
using Lykke.Service.MarketMakerArbitrageDetector.Settings;

namespace Lykke.Service.MarketMakerArbitrageDetector.Rabbit.Subscribers
{
    public class LykkeOrderBookSubscriber : IDisposable
    {
        private const string LykkeExchangeName = "lykke";
        private const string QueueSuffix = "MarketMakerArbitrageDetector";

        private readonly RabbitMqSettings _settings;
        private readonly ILykkeOrderBookHandler[] _lykkeOrderBookHandlers;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        private RabbitMqSubscriber<LykkeOrderBook> _subscriber;

        public LykkeOrderBookSubscriber(
            RabbitMqSettings settings,
            ILykkeOrderBookHandler[] lykkeOrderBookHandlers,
            ILogFactory logFactory)
        {
            _settings = settings;
            _lykkeOrderBookHandlers = lykkeOrderBookHandlers;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.ConnectionString, _settings.Exchange, QueueSuffix);

            settings.DeadLetterExchangeName = null;
            settings.IsDurable = false;

            _subscriber = new RabbitMqSubscriber<LykkeOrderBook>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<LykkeOrderBook>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(LykkeOrderBook message)
        {
            try
            {
                var limitOrders = message.LimitOrders
                    .Select(o => new OrderBookLimitOrder(o.Id, o.ClientId, Math.Abs(o.Volume), o.Price))
                    .ToList();

                var buyLimitOrders = new List<OrderBookLimitOrder>();
                var sellLimitOrders = new List<OrderBookLimitOrder>();

                if (message.IsBuy)
                    buyLimitOrders = limitOrders;
                else
                    sellLimitOrders = limitOrders;

                var lykkeOrderBook = new OrderBook(LykkeExchangeName, new AssetPair(message.AssetPairId), buyLimitOrders, sellLimitOrders, message.Timestamp);

                await Task.WhenAll(_lykkeOrderBookHandlers.Select(o => o.HandleAsync(lykkeOrderBook)));
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during processing lykke order book", message);
            }
        }
    }
}
