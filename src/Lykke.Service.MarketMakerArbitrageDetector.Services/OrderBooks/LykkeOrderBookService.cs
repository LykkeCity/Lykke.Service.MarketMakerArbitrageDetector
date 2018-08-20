using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Service.Assets.Client;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Handlers;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using OrderBook = Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBook;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services.OrderBooks
{
    [UsedImplicitly]
    public class LykkeOrderBookService : ILykkeOrderBookService, ILykkeOrderBookHandler
    {
        private const string LykkeExchangeName = "lykke";
        private readonly IAssetsService _assetsService;
        private readonly IOrderBookProviderClient _orderBookProviderClient;
        private readonly ILog _log;
        private readonly object _sync = new object();

        private readonly Dictionary<string, OrderBook> _lykkeOrderBooks =
            new Dictionary<string, OrderBook>();
        private readonly Dictionary<string, OrderBook> _dirtyLykkeOrderBooks =
            new Dictionary<string, OrderBook>();

        public LykkeOrderBookService(IOrderBookProviderClient orderBookProviderClient, IAssetsService assetsService, ILogFactory logFactory)
        {
            _orderBookProviderClient = orderBookProviderClient;
            _assetsService = assetsService;
            _log = logFactory.CreateLog(this);

            Initialize();
        }

        public IReadOnlyList<OrderBook> GetAll()
        {
            lock (_sync)
            {
                return _lykkeOrderBooks.Values.ToList();
            }
        }

        public Task HandleAsync(OrderBook orderBook)
        {
            lock (_sync)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(orderBook.AssetPair))
                {
                    _dirtyLykkeOrderBooks.Add(orderBook.AssetPair, orderBook);
                }
                else
                {
                    // Update half even if it already exists
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[orderBook.AssetPair];
                    dirtyOrderBook.Timestamp = orderBook.Timestamp;

                    if (orderBook.SellLimitOrders != null)
                        dirtyOrderBook.SellLimitOrders = orderBook.SellLimitOrders;

                    if (orderBook.BuyLimitOrders != null)
                        dirtyOrderBook.BuyLimitOrders = orderBook.BuyLimitOrders;

                    MoveFromDirtyToMain(dirtyOrderBook);
                }
            }

            return Task.CompletedTask;
        }

        private void Initialize()
        {
            Task.Run(async () =>
            {
                await InitializeOrderBooks();
            })
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _log.Error(t.Exception, "Something went wrong during order books initialization from cache provider.");
            });
        }

        private async Task InitializeOrderBooks()
        {
            IList<Assets.Client.Models.AssetPair> assetPairs;
            try
            {
                assetPairs = _assetsService.AssetPairGetAll();
            }
            catch (Exception ex)
            {
                _log.Warning("Can't receive asset pairs from the Lykke.Service.Assets.", ex);
                return;
            }

            var foundOrderBooks = 0;
            foreach (var assetPair in assetPairs)
            {
                Job.OrderBooksCacheProvider.Client.OrderBook providerOrderBook = null;
                try
                {
                    providerOrderBook = await _orderBookProviderClient.GetOrderBookAsync(assetPair.Id);
                    foundOrderBooks++;
                }
                catch { }

                var orderBook = Convert(providerOrderBook);
                AddOrderBookFromCacheProvider(orderBook);
            }
            _log.Info($"Initialized from cache provider {foundOrderBooks} order books out of {assetPairs.Count}.");
        }

        private OrderBook Convert(Job.OrderBooksCacheProvider.Client.OrderBook orderBook)
        {
            if (orderBook == null)
                return null;

            var buyLimitOrders = new List<OrderBookLimitOrder>();
            var sellLimitOrders = new List<OrderBookLimitOrder>();

            foreach (var limitOrder in orderBook.Prices)
            {
                if (limitOrder.Volume > 0)
                    buyLimitOrders.Add(new OrderBookLimitOrder(limitOrder.Id, limitOrder.ClientId, (decimal)limitOrder.Volume, (decimal)limitOrder.Price));
                else
                    sellLimitOrders.Add(new OrderBookLimitOrder(limitOrder.Id, limitOrder.ClientId, Math.Abs((decimal)limitOrder.Volume), (decimal)limitOrder.Price));
            }

            var result = new OrderBook(LykkeExchangeName, orderBook.AssetPair, buyLimitOrders, sellLimitOrders, orderBook.Timestamp);

            return result;
        }

        private void AddOrderBookFromCacheProvider(OrderBook orderBook)
        {
            if (orderBook == null)
                return;

            lock (_sync)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(orderBook.AssetPair))
                {
                    _dirtyLykkeOrderBooks.Add(orderBook.AssetPair, orderBook);
                }
                else
                {
                    // Update half only if it doesn't already exist
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[orderBook.AssetPair];

                    if (dirtyOrderBook.SellLimitOrders == null)
                        dirtyOrderBook.SellLimitOrders = orderBook.SellLimitOrders;

                    if (dirtyOrderBook.BuyLimitOrders == null)
                        dirtyOrderBook.BuyLimitOrders = orderBook.BuyLimitOrders;

                    var changed = dirtyOrderBook.SellLimitOrders == null || dirtyOrderBook.BuyLimitOrders == null;
                    if (changed)
                        dirtyOrderBook.Timestamp = orderBook.Timestamp;

                    MoveFromDirtyToMain(dirtyOrderBook);
                }
            }
        }

        private void MoveFromDirtyToMain(OrderBook dirtyOrderBook)
        {
            if (dirtyOrderBook.SellLimitOrders != null && dirtyOrderBook.BuyLimitOrders != null)
            {
                var isValid = true;

                if (dirtyOrderBook.SellLimitOrders.Any() && dirtyOrderBook.BuyLimitOrders.Any())
                {
                    isValid = dirtyOrderBook.SellLimitOrders.Min(o => o.Price) >
                              dirtyOrderBook.BuyLimitOrders.Max(o => o.Price);
                }

                if (isValid)
                {
                    _lykkeOrderBooks[dirtyOrderBook.AssetPair] =
                        new OrderBook(dirtyOrderBook.Exchange,
                                      dirtyOrderBook.AssetPair,
                                      dirtyOrderBook.BuyLimitOrders,
                                      dirtyOrderBook.SellLimitOrders,
                                      dirtyOrderBook.Timestamp);
                }
            }
        }
    }
}
