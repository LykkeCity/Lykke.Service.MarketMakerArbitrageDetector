using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services
{
    public class ArbitrageDetectorService : IArbitrageDetectorService, IStartable, IStopable
    {
        private readonly object _sync = new object();
        private readonly List<Arbitrage> _arbitrages = new List<Arbitrage>();

        private readonly ISettingsService _settingsService;
        private readonly IOrderBooksService _orderBooksService;
        private readonly TimerTrigger _trigger;
        private readonly ILog _log;

        public ArbitrageDetectorService(ISettingsService settingsService, IOrderBooksService orderBooksService, ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _orderBooksService = orderBooksService ?? throw new ArgumentNullException(nameof(orderBooksService));
            _log = logFactory.CreateLog(this);

            var executionInterval = _settingsService.Get().ExecutionInterval;
            _trigger = new TimerTrigger(nameof(ArbitrageDetectorService), executionInterval, logFactory, Execute);
        }

        public IReadOnlyCollection<Arbitrage> GetAll(string target, string source)
        {
            IEnumerable<Arbitrage> copy;
            lock (_sync)
            {
                copy = _arbitrages.ToList();
            }

            var result = new List<Arbitrage>();

            // Filter by target
            if (!string.IsNullOrWhiteSpace(target))
                copy = copy.Where(x => x.Target.Name.Equals(target, StringComparison.OrdinalIgnoreCase)).ToList();

            var groupedByTarget = copy.GroupBy(x => x.Target);
            foreach (var targetPairArbitrages in groupedByTarget)
            {
                var targetArbitrages = targetPairArbitrages.ToList();

                // No target
                if (string.IsNullOrWhiteSpace(target))
                {
                    // Best arbitrage for each target
                    var bestByProperty = targetArbitrages.MaxBy(x => x.PnL);
                    bestByProperty.SourcesCount = targetArbitrages.Count;
                    result.Add(bestByProperty);
                }
                // Target selected
                else
                {
                    // No source selected
                    if (string.IsNullOrWhiteSpace(source))
                    {
                        // Group by source
                        var groupedBySource = targetArbitrages.GroupBy(x => x.Source);

                        foreach (var group in groupedBySource)
                        {
                            // Best arbitrage by target and source
                            var targetGrouped = group.ToList();
                            var bestByProperty = targetGrouped.MaxBy(x => x.PnL);
                            bestByProperty.SynthsCount = targetGrouped.Count;
                            result.Add(bestByProperty);
                        }
                    }
                    // Source selected
                    else
                    {
                        // Filter by source
                        targetArbitrages = targetArbitrages.Where(x => x.Source.Name.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();

                        var groupedBySource = targetArbitrages.GroupBy(x => x.Source);
                        foreach (var baseSourcePairsArbitrages in groupedBySource)
                        {
                            var baseSourceArbitrages = baseSourcePairsArbitrages.ToList();
                            result.AddRange(baseSourceArbitrages);
                        }
                    }
                }
            }

            result = result.OrderByDescending(x => x.PnLInUsd)
                           .ThenByDescending(x => x.VolumeInUsd)
                           .ThenBy(x => x.Spread)
                           .ToList();

            return result;
        }

        public Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return Task.CompletedTask;
        }

        private void Execute()
        {
            var orderBooks = GetOrderBooksWithWantedLimitOrders();
            var lykkeArbitrages = GetArbitrages(orderBooks);
            RefreshArbitrages(lykkeArbitrages);
        }

        private IEnumerable<Arbitrage> GetArbitrages(IReadOnlyCollection<OrderBook> orderBooks)
        {
            orderBooks = orderBooks.Where(x => x.BestBid != null || x.BestAsk != null).ToList();

            var result = new List<Arbitrage>();

            var watch = Stopwatch.StartNew();

            var synthsCount = 0;
            var totalItarations = 0;
            // O( (n^2)/2 )
            for (var i = 0; i < orderBooks.Count; i++)
            {
                if (i == orderBooks.Count - 1)
                    break;

                var target = orderBooks.ElementAt(i);

                for (var j = i + 1; j < orderBooks.Count; j++)
                {
                    var source = orderBooks.ElementAt(j);

                    if (target.ToString() == source.ToString())
                        continue;

                    // Calculate all synthetic order books between source order book and target order book
                    var synthOrderBooks = SynthOrderBook.GetSynthsFromAll(target.AssetPair, source, orderBooks);
                    synthsCount += synthOrderBooks.Count;

                    // Compare each synthetic with target
                    foreach (var synthOrderBook in synthOrderBooks)
                    {
                        totalItarations++;

                        decimal spread = 0;
                        decimal volume = 0;
                        decimal pnL = 0;
                        string targetSide = null;

                        //TODO: Can be rewritten
                        if (target.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(target.BestBid.Price, synthOrderBook.BestAsk.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(target.Bids, synthOrderBook.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every found arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every found arbitrage must have PnL");
                            targetSide = "Bid";
                        }

                        if (synthOrderBook.BestBid?.Price > target.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Price, target.BestAsk.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(synthOrderBook.Bids, target.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every found arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every found arbitrage must have PnL");
                            targetSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(targetSide)) // no arbitrages
                            continue;

                        var volumeInUsd = _orderBooksService.ConvertToUsd(target.AssetPair.Base.Id, volume);
                        var pnLInUsd = _orderBooksService.ConvertToUsd(target.AssetPair.Quote.Id, pnL);

                        var lykkeArbitrage = new Arbitrage (
                            target.AssetPair,
                            source.AssetPair,
                            spread,
                            targetSide,
                            synthOrderBook.ConversionPath,
                            volume,
                            volumeInUsd,
                            pnL,
                            pnLInUsd,
                            target.BestBid?.Price,
                            target.BestAsk?.Price,
                            synthOrderBook.BestBid?.Price,
                            synthOrderBook.BestAsk?.Price
                        );
                        result.Add(lykkeArbitrage);
                    }
                }
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                _log.Info($"{watch.ElapsedMilliseconds} ms, {result.Count} arbitrages, {orderBooks.Count} order books, {synthsCount} synthetic order books created, {totalItarations} iterations.");

            return result.OrderByDescending(x => x.PnL).ToList();
        }

        private void RefreshArbitrages(IEnumerable<Arbitrage> lykkeArbitrages)
        {
            lock (_sync)
            {
                _arbitrages.Clear();
                _arbitrages.AddRange(lykkeArbitrages);
            }
        }

        private IReadOnlyCollection<OrderBook> GetOrderBooksWithWantedLimitOrders()
        {
            var allOrderBooks = _orderBooksService.GetAll();

            var wallets = _settingsService.Get().Wallets;
            if (!wallets.Any())
                return allOrderBooks;

            var result = new List<OrderBook>();
            
            foreach (var orderBook in allOrderBooks)
            {
                var newBids = orderBook.Bids.Where(x => wallets.Keys.Contains(x.WalletId)).ToList();
                var newAsks = orderBook.Asks.Where(x => wallets.Keys.Contains(x.WalletId)).ToList();

                if (!newBids.Any() && !newAsks.Any())
                    continue;

                var newOrderBook = new OrderBook(orderBook.Exchange, orderBook.AssetPair, newBids, newAsks, orderBook.Timestamp);
                result.Add(newOrderBook);
            }

            return result;
        }

        #region IStartable, IStopable

        public void Start()
        {
            _trigger.Start();
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();
        }

        #endregion
    }
}
