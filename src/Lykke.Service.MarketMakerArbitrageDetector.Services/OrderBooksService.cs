using System;
using System.Collections.Concurrent;
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
using MoreLinq;
using OrderBook = Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBook;
using CacheProviderOrderBook = Lykke.Job.OrderBooksCacheProvider.Client.OrderBook;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services
{
    [UsedImplicitly]
    public class OrderBooksService : IOrderBooksService, ILykkeOrderBookHandler
    {
        private const string LykkeExchangeName = "lykke";

        private readonly ISettingsService _settingsService;
        private readonly IAssetsService _assetsService;
        private readonly IOrderBookProviderClient _orderBookProviderClient;
        private readonly ILog _log;

        private readonly ConcurrentDictionary<string, Asset> _assets = new ConcurrentDictionary<string, Asset>();
        private readonly ConcurrentDictionary<string, AssetPair> _assetPairs = new ConcurrentDictionary<string, AssetPair>();

        private readonly object _sync = new object();
        private readonly Dictionary<string, OrderBook> _lykkeOrderBooks = new Dictionary<string, OrderBook>();
        private readonly Dictionary<string, OrderBook> _dirtyLykkeOrderBooks = new Dictionary<string, OrderBook>();

        public OrderBooksService(ISettingsService settingsService, IOrderBookProviderClient orderBookProviderClient,
            IAssetsService assetsService, ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _orderBookProviderClient = orderBookProviderClient;
            _assetsService = assetsService;
            _log = logFactory.CreateLog(this);

            Initialize();
        }

        public IReadOnlyCollection<OrderBook> GetAll()
        {
            lock (_sync)
            {
                return _lykkeOrderBooks.Values.OrderBy(x => x.AssetPair.Name).ToList();
            }
        }

        public IReadOnlyCollection<OrderBookRow> GetAllRows()
        {
            lock (_sync)
            {
                return _lykkeOrderBooks.Values.Select(x =>
                    new OrderBookRow
                    (
                        x.Exchange,
                        x.AssetPair.Name,
                        GetMarketMakers(x).Values,
                        x.BestBid?.Price,
                        x.BestAsk?.Price,
                        x.BidsVolume,
                        ConvertToUsd(x.AssetPair.Base.Id, x.BidsVolume), 
                        x.AsksVolume,
                        ConvertToUsd(x.AssetPair.Base.Id, x.AsksVolume),
                        x.Timestamp
                    )).OrderBy(x => x.AssetPair).ToList();
            }
        }

        public OrderBook Get(string assetPairId)
        {
            lock (_sync)
            {
                return _lykkeOrderBooks.Values.SingleOrDefault(x => x.AssetPair.Id == assetPairId);
            }
        }

        public Task HandleAsync(OrderBook orderBook)
        {
            if (!_assetPairs.ContainsKey(orderBook.AssetPair.Id))
                return Task.CompletedTask;

            orderBook.SetAssetPair(_assetPairs[orderBook.AssetPair.Id]);

            lock (_sync)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(orderBook.AssetPair.Id))
                {
                    _dirtyLykkeOrderBooks.Add(orderBook.AssetPair.Id, orderBook);
                }
                else
                {
                    // Update half even if it already exists
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[orderBook.AssetPair.Id];

                    var newBids = orderBook.Bids ?? dirtyOrderBook.Bids;
                    var newAsks = orderBook.Asks ?? dirtyOrderBook.Asks;

                    var newOrderBook = new OrderBook(orderBook.Exchange, orderBook.AssetPair, newBids, newAsks, orderBook.Timestamp);
                    _dirtyLykkeOrderBooks[orderBook.AssetPair.Id] = newOrderBook;
                }

                MoveFromDirtyToMain(orderBook.AssetPair.Id);
            }

            return Task.CompletedTask;
        }

        public decimal? ConvertToUsd(string sourceAssetId, decimal value, int accuracy = 0)
        {
            if (value == 0)
                return 0;

            var multiplier = ConvertToUsd(sourceAssetId);
            if (multiplier == null)
                return null;

            return Math.Round(multiplier.Value * value, accuracy);
        }

        private decimal? ConvertToUsd(string sourceAssetId)
        {
            if (string.IsNullOrWhiteSpace(sourceAssetId))
                throw new ArgumentException(nameof(sourceAssetId));

            var usd = _assets.Values.Single(x => x.Id == "USD");
            var asset = _assets.Values.Single(x => x.Id == sourceAssetId);
            
            if (sourceAssetId == usd.Id)
                return 1;

            // Get with the shortest id because it can be EURUSD and EURUSD.CY
            var assetPair = _assetPairs.Values.Where(x => x.Base.Id == asset.Id && x.Quote.Id == usd.Id).OrderBy(x => x.Id).FirstOrDefault();
            if (assetPair != null)
            {
                lock (_sync)
                {
                    if (!_lykkeOrderBooks.ContainsKey(assetPair.Id))
                        return null;

                    var orderBook = _lykkeOrderBooks[assetPair.Id];
                    return orderBook.BestAsk?.Price;
                }
            }

            assetPair = _assetPairs.Values.Where(x => x.Base.Id == usd.Id && x.Quote.Id == asset.Id).OrderBy(x => x.Id).FirstOrDefault();
            if (assetPair == null)
                return null;

            lock (_sync)
            {
                if (!_lykkeOrderBooks.ContainsKey(assetPair.Id))
                    return null;

                var orderBook = _lykkeOrderBooks[assetPair.Id];
                return orderBook.BestBid?.Reciprocal().Price;
            }
        }

        private void Initialize()
        {
            InitializeAssets();
            InitializeAssetPairs();

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

        private void InitializeAssets()
        {
            var assets = _assetsService.AssetGetAll();
            foreach (var asset in assets)
            {
                _assets[asset.Id] = new Asset(asset.Id, GetShortestName(asset.Id, asset.Name, asset.DisplayId));
            }

            _log.Info($"Initialized {_assets.Count} of {assets.Count} assets.");
        }

        private void InitializeAssetPairs()
        {
            var assetPairs = _assetsService.AssetPairGetAll();
            foreach (var assetPair in assetPairs)
            {
                var baseAsset = _assets.Values.SingleOrDefault(x => x.Id == assetPair.BaseAssetId);
                var quoteAsset = _assets.Values.SingleOrDefault(x => x.Id == assetPair.QuotingAssetId);

                if (baseAsset == null || quoteAsset == null)
                    continue;

                _assetPairs[assetPair.Id] = new AssetPair(assetPair.Id, baseAsset.Name + quoteAsset.Name, baseAsset, quoteAsset);
            }

            _log.Info($"Initialized {_assetPairs.Count} of {assetPairs.Count} asset pairs.");
        }

        private async Task InitializeOrderBooks()
        {
            var assetPairs = _assetPairs.Values.ToList();

            var foundOrderBooks = 0;
            foreach (var assetPair in assetPairs)
            {
                CacheProviderOrderBook providerOrderBook = null;
                try
                {
                    providerOrderBook = await _orderBookProviderClient.GetOrderBookAsync(assetPair.Id);
                    foundOrderBooks++;
                }
                catch (Exception)
                {
                    // Some order books can't be found by asset pair id
                }

                if (providerOrderBook == null)
                    continue;

                var orderBook = Convert(providerOrderBook);
                AddOrderBookFromCacheProvider(orderBook);
            }

            _log.Info($"Initialized {foundOrderBooks} of {assetPairs.Count} order books. For now {_dirtyLykkeOrderBooks.Count} order books.");
        }

        private void AddOrderBookFromCacheProvider(OrderBook orderBook)
        {
            if (!_assetPairs.ContainsKey(orderBook.AssetPair.Id))
                return;

            orderBook.SetAssetPair(_assetPairs[orderBook.AssetPair.Id]);

            lock (_sync)
            {
                if (!_dirtyLykkeOrderBooks.ContainsKey(orderBook.AssetPair.Id))
                {
                    _dirtyLykkeOrderBooks.Add(orderBook.AssetPair.Id, orderBook);
                }
                else
                {
                    // Update half only if it doesn't exist
                    var dirtyOrderBook = _dirtyLykkeOrderBooks[orderBook.AssetPair.Id];

                    var hasToBeUpdated = dirtyOrderBook.Bids == null && orderBook.Bids != null ||
                                         dirtyOrderBook.Asks == null && orderBook.Asks != null;

                    if (!hasToBeUpdated)
                        return;

                    var newBids = dirtyOrderBook.Bids ?? orderBook.Bids;
                    var newAsks = dirtyOrderBook.Asks ?? orderBook.Asks;

                    var newOrderBook = new OrderBook(orderBook.Exchange, orderBook.AssetPair, newBids, newAsks, orderBook.Timestamp);
                    _dirtyLykkeOrderBooks[orderBook.AssetPair.Id] = newOrderBook;
                }

                MoveFromDirtyToMain(orderBook.AssetPair.Id);
            }
        }

        private void MoveFromDirtyToMain(string assetPairId)
        {
            var dirtyOrderBook = _dirtyLykkeOrderBooks[assetPairId];

            if (dirtyOrderBook.Asks != null && dirtyOrderBook.Bids != null)
            {
                var isValid = true;
                
                // Only if both bids and asks not empty
                if (dirtyOrderBook.Asks.Any() && dirtyOrderBook.Bids.Any())
                {
                    isValid = dirtyOrderBook.Asks.Min(o => o.Price) >
                              dirtyOrderBook.Bids.Max(o => o.Price);
                }

                if (isValid)
                {
                    _lykkeOrderBooks[dirtyOrderBook.AssetPair.Id] =
                        new OrderBook(dirtyOrderBook.Exchange, dirtyOrderBook.AssetPair, dirtyOrderBook.Bids, dirtyOrderBook.Asks, dirtyOrderBook.Timestamp);
                }
            }
        }

        private Dictionary<string, string> GetMarketMakers(OrderBook orderBook)
        {
            var result = new Dictionary<string, string>();

            var wallets = _settingsService.Get().Wallets;
            var clientIds = wallets.Keys;

            if (!wallets.Any())
                return result;

            var bidsClientsIds = orderBook.Bids.Select(x => x.ClientId).Intersect(clientIds);
            var asksClientIds = orderBook.Asks.Select(x => x.ClientId).Intersect(clientIds);
            var allFoundClientIds = bidsClientsIds.Union(asksClientIds);

            foreach (var clientId in allFoundClientIds)
                result[clientId] = wallets[clientId];

            return result;
        }

        private static OrderBook Convert(CacheProviderOrderBook orderBook)
        {
            var bids = new List<LimitOrder>();
            var asks = new List<LimitOrder>();

            foreach (var limitOrder in orderBook.Prices)
            {
                if (limitOrder.Volume > 0)
                    bids.Add(new LimitOrder(limitOrder.Id, limitOrder.ClientId, (decimal)limitOrder.Price, (decimal)limitOrder.Volume));
                else
                    asks.Add(new LimitOrder(limitOrder.Id, limitOrder.ClientId, (decimal)limitOrder.Price, Math.Abs((decimal)limitOrder.Volume)));
            }

            // Filter out negative and zero prices and volumes
            bids = bids.Where(x => x.Price > 0 && x.Volume > 0).ToList();
            asks = asks.Where(x => x.Price > 0 && x.Volume > 0).ToList();

            var result = new OrderBook(LykkeExchangeName, new AssetPair(orderBook.AssetPair), bids, asks, orderBook.Timestamp);

            return result;
        }

        private static string GetShortestName(string id, string name, string displayId)
        {
            var allNames = new List<string> { id, name, displayId };
            return allNames.Where(x => x != null).MinBy(x => x.Length);
        }
    }
}
