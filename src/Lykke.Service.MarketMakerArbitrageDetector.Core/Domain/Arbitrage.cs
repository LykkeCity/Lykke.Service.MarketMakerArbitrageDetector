using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static (decimal Volume, decimal PnL)? GetArbitrageVolumePnL(IEnumerable<LimitOrder> orderedBids, IEnumerable<LimitOrder> orderedAsks)
        {
            Debug.Assert(orderedBids != null);
            Debug.Assert(orderedAsks != null);

            var orderedBidsEnumerator = orderedBids.GetEnumerator();
            var orderedAsksEnumerator = orderedAsks.GetEnumerator();

            if (!orderedBidsEnumerator.MoveNext() || !orderedAsksEnumerator.MoveNext())
                return null;

            // Clone bids and asks
            decimal volume = 0;
            decimal pnl = 0;
            var bid = orderedBidsEnumerator.Current.CloneWithoutIds();
            var ask = orderedAsksEnumerator.Current.CloneWithoutIds();
            while (true)
            {
                if (bid.Price <= ask.Price)
                    break;
                
                var tradeBidPrice = bid.Price;
                var tradeAskPrice = ask.Price;
                if (bid.Volume < ask.Volume)
                {
                    volume += bid.Volume;
                    pnl += bid.Volume * (tradeBidPrice - tradeAskPrice);
                    ask.Volume = ask.Volume - bid.Volume;

                    if (!orderedBidsEnumerator.MoveNext()) break;
                    bid = orderedBidsEnumerator.Current;
                }
                else if (bid.Volume > ask.Volume)
                {
                    volume += ask.Volume;
                    pnl += ask.Volume * (tradeBidPrice - tradeAskPrice);
                    bid.Volume = bid.Volume - ask.Volume;

                    if (!orderedAsksEnumerator.MoveNext()) break;
                    ask = orderedAsksEnumerator.Current;
                }
                else if (bid.Volume == ask.Volume)
                {
                    volume += bid.Volume;
                    pnl += bid.Volume * (tradeBidPrice - tradeAskPrice);

                    if (!orderedBidsEnumerator.MoveNext()) break;
                    bid = orderedBidsEnumerator.Current;
                    if (!orderedAsksEnumerator.MoveNext()) break;
                    ask = orderedAsksEnumerator.Current;
                }
            }

            orderedBidsEnumerator.Dispose();
            orderedAsksEnumerator.Dispose();

            return volume == 0 ? ((decimal, decimal)?)null : (volume, Math.Round(pnl, 8));
        }

        public override string ToString()
        {
            return Target + "-" + Source + " : " + ConversionPath;
        }
    }
}
