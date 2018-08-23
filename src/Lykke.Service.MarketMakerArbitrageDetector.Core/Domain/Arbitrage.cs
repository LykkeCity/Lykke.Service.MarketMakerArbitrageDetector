using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class Arbitrage
    {
        public AssetPair Target { get; }

        public AssetPair Source { get; }

        public decimal Spread { get; }

        public string TargetSide { get; }

        public string ConversionPath { get; }

        public decimal Volume { get; }

        public decimal? VolumeInUsd { get; }

        public decimal PnL { get; }

        public decimal? PnLInUsd { get; }

        public decimal? BaseAsk { get; }

        public decimal? BaseBid { get; }

        public decimal? SynthAsk { get; }

        public decimal? SynthBid { get; }

        public Arbitrage(AssetPair baseAssetPair, AssetPair crossAssetPair, decimal spread, string baseSide,
            string conversionPath, decimal volume, decimal? baseBid, decimal? baseAsk, decimal? synthBid, decimal? synthAsk, decimal? volumeInUsd,
            decimal pnL, decimal? pnLInUsd)
        {
            Target = !baseAssetPair.IsValid() ? throw new ArgumentNullException(nameof(baseAssetPair)) : baseAssetPair;
            Source = !crossAssetPair.IsValid() ? throw new ArgumentNullException(nameof(crossAssetPair)) : crossAssetPair;
            Spread = spread;
            TargetSide = string.IsNullOrWhiteSpace(baseSide) ? throw new ArgumentNullException(nameof(baseSide)) : baseSide;
            ConversionPath = string.IsNullOrWhiteSpace(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
            Volume = volume;
            BaseAsk = baseAsk;
            BaseBid = baseBid;
            SynthAsk = synthAsk;
            SynthBid = synthBid;
            VolumeInUsd = volumeInUsd;
            PnL = pnL;
            PnLInUsd = pnLInUsd;
        }

        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return (askPrice - bidPrice) / bidPrice * 100;
        }

        public static decimal GetPnL(decimal bidPrice, decimal askPrice, decimal volume)
        {
            return (bidPrice - askPrice) * volume;
        }

        public static (decimal? Volume, decimal? PnL)? GetArbitrageVolumePnL(IReadOnlyCollection<LimitOrder> bids, IReadOnlyCollection<LimitOrder> asks)
        {
            if (bids == null)
                throw new ArgumentException($"{nameof(bids)}");

            if (asks == null)
                throw new ArgumentException($"{nameof(asks)}");

            if (!bids.Any() || !asks.Any() || bids.Max(x => x.Price) <= asks.Min(x => x.Price))
                return null; // no arbitrage

            // Clone bids and asks (that in arbitrage only)
            var currentBids = new List<LimitOrder>();
            var currentAsks = new List<LimitOrder>();
            currentBids.AddRange(bids);
            currentAsks.AddRange(asks);

            decimal volume = 0;
            decimal pnl = 0;
            do
            {
                // Recalculate best bid and best ask
                var bestBidPrice = currentBids.Max(x => x.Price);
                var bestAskPrice = currentAsks.Min(x => x.Price);
                currentBids = currentBids.Where(x => x.Price > bestAskPrice).OrderByDescending(x => x.Price).ToList();
                currentAsks = currentAsks.Where(x => x.Price < bestBidPrice).OrderBy(x => x.Price).ToList();

                if (!currentBids.Any() || !currentAsks.Any()) // no more arbitrage
                    break;

                var bid = currentBids.First();
                var ask = currentAsks.First();

                // Calculate volume for current step and remove it
                decimal currentVolume = 0;
                if (bid.Volume > ask.Volume)
                {
                    currentVolume = ask.Volume;
                    var newBidVolume = bid.Volume - ask.Volume;
                    var newBid = new LimitOrder(bid.ClientId, bid.OrderId, bid.Price, newBidVolume);
                    currentBids.Remove(bid);
                    currentBids.Insert(0, newBid);
                    currentAsks.Remove(ask);
                }

                if (bid.Volume < ask.Volume)
                {
                    currentVolume = bid.Volume;
                    var newAskVolume = ask.Volume - bid.Volume;
                    var newAsk = new LimitOrder(ask.ClientId, ask.OrderId, ask.Price, newAskVolume);
                    currentAsks.Remove(ask);
                    currentAsks.Insert(0, newAsk);
                    currentBids.Remove(bid);
                }

                if (bid.Volume == ask.Volume)
                {
                    currentVolume = bid.Volume;
                    currentBids.Remove(bid);
                    currentAsks.Remove(ask);
                }

                volume += currentVolume;
                pnl += currentVolume * (bid.Price - ask.Price);
            }
            while (currentBids.Any() && currentAsks.Any());

            return volume == 0 ? ((decimal?, decimal?)?)null : (volume, pnl);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
