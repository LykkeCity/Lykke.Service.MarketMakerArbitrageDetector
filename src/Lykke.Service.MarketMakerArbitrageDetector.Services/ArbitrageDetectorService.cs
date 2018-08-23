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

        public IReadOnlyCollection<Arbitrage> GetAll()
        {
            lock (_sync)
            {
                return _arbitrages;
            }
        }

        public Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
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
            var orderBooks = _orderBooksService.GetAll();
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

                        if (target.BestBid?.Price > synthOrderBook.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(target.BestBid.Price, synthOrderBook.BestAsk.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(target.Bids, synthOrderBook.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Bid";
                        }

                        if (synthOrderBook.BestBid?.Price > target.BestAsk?.Price)
                        {
                            spread = Arbitrage.GetSpread(synthOrderBook.BestBid.Price, target.BestAsk.Price);
                            var volumePnL = Arbitrage.GetArbitrageVolumePnL(synthOrderBook.Bids, target.Asks);
                            volume = volumePnL?.Volume ?? throw new InvalidOperationException("Every arbitrage must have volume");
                            pnL = volumePnL?.PnL ?? throw new InvalidOperationException("Every arbitrage must have PnL");
                            targetSide = "Ask";
                        }

                        if (string.IsNullOrWhiteSpace(targetSide)) // no arbitrages
                            continue;

                        var baseToUsdRate = _orderBooksService.ConvertToUsd(target.AssetPair.Base.Id);
                        var quoteToUsdRate = _orderBooksService.ConvertToUsd(target.AssetPair.Quote.Id);
                        var volumeInUsd = volume * baseToUsdRate;
                        volumeInUsd = volumeInUsd.HasValue ? Math.Round(volumeInUsd.Value) : (decimal?)null;
                        var pnLInUsd = pnL * quoteToUsdRate;
                        pnLInUsd = pnLInUsd.HasValue ? Math.Round(pnLInUsd.Value) : (decimal?)null;

                        var lykkeArbitrage = new Arbitrage(target.AssetPair, source.AssetPair, spread, targetSide, synthOrderBook.ConversionPath,
                            volume, target.BestBid?.Price, target.BestAsk?.Price, synthOrderBook.BestBid?.Price, synthOrderBook.BestAsk?.Price, volumeInUsd,
                            pnL, pnLInUsd);
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
