using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class Arbitrage
    {
        public AssetPair Target { get; }

        public AssetPair Source { get; }

        public int SourcesCount { get; set; }

        public int SynthsCount { get; set; }

        public decimal Spread { get; }

        public string TargetSide { get; }

        public string ConversionPath { get; }

        public decimal Volume { get; }

        public decimal? VolumeInUsd { get; }

        public decimal PnL { get; }

        public decimal? PnLInUsd { get; }

        public decimal? TargetAsk { get; }

        public decimal? TargetBid { get; }

        public decimal? SynthAsk { get; }

        public decimal? SynthBid { get; }


        public Arbitrage(AssetPair target, AssetPair source, decimal spread, string targetSide, string conversionPath,
            decimal volume, decimal? volumeInUsd, decimal pnL, decimal? pnLInUsd, decimal? targetBid, decimal? targetAsk,
            decimal? synthBid, decimal? synthAsk)
        {
            Target = target;
            Source = source;
            Spread = spread;
            TargetSide = targetSide;
            ConversionPath = conversionPath;
            Volume = volume;
            VolumeInUsd = volumeInUsd;
            PnL = pnL;
            PnLInUsd = pnLInUsd;
            TargetBid = targetBid;
            TargetAsk = targetAsk;
            SynthBid = synthBid;
            SynthAsk = synthAsk;
        }

        public static decimal GetSpread(decimal bidPrice, decimal askPrice)
        {
            return Math.Round((askPrice - bidPrice) / bidPrice * 100, 2);
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
            var bestBidPrice = bids.Max(x => x.Price);
            var bestAskPrice = asks.Min(x => x.Price);
            var currentBids = bids.Where(x => x.Price > bestAskPrice)
                                  .Select(x => new LimitOrder(x.OrderId, x.ClientId, x.Price, x.Volume))
                                  .OrderByDescending(x => x.Price).ToList();
            var currentAsks = asks.Where(x => x.Price < bestBidPrice)
                                  .Select(x => new LimitOrder(x.OrderId, x.ClientId, x.Price, x.Volume))
                                  .OrderBy(x => x.Price).ToList();
            decimal volume = 0;
            decimal pnl = 0;
            do
            {
                var bid = currentBids.First();
                var ask = currentAsks.First();

                if (!currentBids.Any() || !currentAsks.Any() || bid.Price <= ask.Price) // no more arbitrage
                    break;

                // Calculate volume for current step and remove it
                decimal currentVolume = 0;
                if (bid.Volume > ask.Volume)
                {
                    currentVolume = ask.Volume;
                    var newBidVolume = bid.Volume - ask.Volume;
                    var newBid = new LimitOrder(bid.OrderId, bid.ClientId, bid.Price, newBidVolume);
                    currentBids.Remove(bid);
                    currentBids.Insert(0, newBid);
                    currentAsks.Remove(ask);
                }

                if (bid.Volume < ask.Volume)
                {
                    currentVolume = bid.Volume;
                    var newAskVolume = ask.Volume - bid.Volume;
                    var newAsk = new LimitOrder(ask.OrderId, ask.ClientId, ask.Price, newAskVolume);
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

            return volume == 0 ? ((decimal?, decimal?)?)null : (volume, Math.Round(pnl, 8));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
