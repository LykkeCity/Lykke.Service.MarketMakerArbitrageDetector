using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class SynthOrderBook
    {
        public string Exchange => string.Join(" - ", OriginalOrderBooks.Select(x => x.Exchange));

        public AssetPair AssetPair { get; }

        public IEnumerable<LimitOrder> Bids => GetPrices(GetBids);

        public IEnumerable<LimitOrder> Asks => GetPrices(GetAsks);

        public LimitOrder BestBid => Bids.FirstOrDefault();

        public LimitOrder BestAsk => Asks.FirstOrDefault();

        public IReadOnlyList<OrderBook> OriginalOrderBooks { get; }

        public string ConversionPath => string.Join(" & ", OriginalOrderBooks.Select(x => x.AssetPair.Name));

        public DateTime Timestamp => OriginalOrderBooks.Select(x => x.Timestamp).Min();


        public SynthOrderBook(AssetPair assetPair, IReadOnlyList<OrderBook> originalOrderBooks)
        {
            AssetPair = assetPair;
            OriginalOrderBooks = originalOrderBooks;
        }


        public static SynthOrderBook FromOrderBook(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(target != null && target.IsValid());
            Debug.Assert(target.EqualOrReversed(orderBook.AssetPair));

            var result = new SynthOrderBook(target, new List<OrderBook> { orderBook });

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook first, OrderBook second, AssetPair target)
        {
            Debug.Assert(first != null);
            Debug.Assert(first.AssetPair != null && first.AssetPair.IsValid());
            Debug.Assert(second != null);
            Debug.Assert(second.AssetPair != null && second.AssetPair.IsValid());
            Debug.Assert(target != null && target.IsValid());

            var result = new SynthOrderBook(target, GetOrdered(new List<OrderBook> { first, second }, target));

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook first, OrderBook second, OrderBook third, AssetPair target)
        {
            Debug.Assert(first != null);
            Debug.Assert(first.AssetPair != null && first.AssetPair.IsValid());
            Debug.Assert(second != null);
            Debug.Assert(second.AssetPair != null && second.AssetPair.IsValid());
            Debug.Assert(third != null);
            Debug.Assert(third.AssetPair != null && third.AssetPair.IsValid());
            Debug.Assert(target != null && target.IsValid());

            var result = new SynthOrderBook(target, GetOrdered(new List<OrderBook> { first, second, third }, target));

            return result;
        }


        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom1(AssetPair target,
            IReadOnlyCollection<OrderBook> sourceOrderBooks, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            // Trying to find base asset in current source's asset pair
            var withBaseOrQuoteOrderBooks = sourceOrderBooks.Where(x => x.AssetPair.ContainsAsset(target.Base.Id) ||
                                                                        x.AssetPair.ContainsAsset(target.Quote.Id)).ToList();

            foreach (var withBaseOrQuoteOrderBook in withBaseOrQuoteOrderBooks)
            {
                var withBaseOrQuoteAssetPair = withBaseOrQuoteOrderBook.AssetPair;

                // Get intermediate asset
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAssetId(target.Base.Id)
                                  ?? withBaseOrQuoteAssetPair.GetOtherAssetId(target.Quote.Id);

                // If current is target or reversed then just use it
                if (intermediateId == target.Base.Id || intermediateId == target.Quote.Id)
                {
                    if (!withBaseOrQuoteOrderBook.Asks.Any() && !withBaseOrQuoteOrderBook.Bids.Any())
                        continue;

                    var synthOrderBook = FromOrderBook(withBaseOrQuoteOrderBook, target);
                    result.Add(synthOrderBook);
                }
            }

            return result;
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom2(AssetPair target,
            IReadOnlyCollection<OrderBook> sourceOrderBooks, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            // Trying to find base asset in current source's asset pair
            var withBaseOrQuoteOrderBooks = sourceOrderBooks.Where(x => x.AssetPair.ContainsAsset(target.Base.Id) ||
                                                                        x.AssetPair.ContainsAsset(target.Quote.Id)).ToList();

            foreach (var withBaseOrQuoteOrderBook in withBaseOrQuoteOrderBooks)
            {
                var withBaseOrQuoteAssetPair = withBaseOrQuoteOrderBook.AssetPair;

                // Get intermediate asset
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAssetId(target.Base.Id)
                                ?? withBaseOrQuoteAssetPair.GetOtherAssetId(target.Quote.Id);

                // 1. If current is target or reversed then just use it
                if (intermediateId == target.Base.Id || intermediateId == target.Quote.Id)
                    continue; // The pairs are the same or reversed (it is from 1 order book)

                // 1. If current is base&intermediate then find quote&intermediate
                if (withBaseOrQuoteAssetPair.ContainsAsset(target.Base.Id))
                {
                    var baseAndIntermediate = withBaseOrQuoteOrderBook;
                    // Trying to find quote/intermediate or intermediate/quote pair (quote&intermediate)
                    var intermediateQuoteOrderBooks = allOrderBooks
                        .Where(x => x.AssetPair.ContainsAsset(intermediateId) && x.AssetPair.ContainsAsset(target.Quote.Id))
                        .ToList();

                    foreach (var intermediateQuoteOrderBook in intermediateQuoteOrderBooks)
                    {
                        if (!baseAndIntermediate.Asks.Any() && !baseAndIntermediate.Bids.Any()
                            || !intermediateQuoteOrderBook.Asks.Any() && !intermediateQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(baseAndIntermediate, intermediateQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }

                // 2. If current is quote&intermediate then find base&intermediate
                if (withBaseOrQuoteAssetPair.ContainsAsset(target.Quote.Id))
                {
                    var quoteAndIntermediate = withBaseOrQuoteOrderBook;
                    // Trying to find base/intermediate or intermediate/base pair (base&intermediate)
                    var intermediateBaseOrderBooks = allOrderBooks
                        .Where(x => x.AssetPair.ContainsAsset(intermediateId) && x.AssetPair.ContainsAsset(target.Base.Id))
                        .ToList();

                    foreach (var intermediateBaseOrderBook in intermediateBaseOrderBooks)
                    {
                        if (!intermediateBaseOrderBook.Asks.Any() && !intermediateBaseOrderBook.Bids.Any()
                            || !quoteAndIntermediate.Asks.Any() && !quoteAndIntermediate.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(intermediateBaseOrderBook, quoteAndIntermediate, target);
                        result.Add(synthOrderBook);
                    }
                }
            }

            return result;
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom3(AssetPair target,
            IReadOnlyCollection<OrderBook> sourceOrderBooks, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            var woBaseAndQuoteOrderBooks = sourceOrderBooks
                .Where(x => !x.AssetPair.ContainsAsset(target.Base.Id)
                         && !x.AssetPair.ContainsAsset(target.Quote.Id)).ToList();

            foreach (var woBaseAndQuoteOrderBook in woBaseAndQuoteOrderBooks)
            {
                // Get assets from order book
                var @base = woBaseAndQuoteOrderBook.AssetPair.Base;
                var quote = woBaseAndQuoteOrderBook.AssetPair.Quote;

                // Trying to find pair from @base to target.Base and quote to target.Quote
                var baseTargetBaseOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(@base.Id, target.Base.Id)).ToList();
                foreach (var baseTargetBaseOrderBook in baseTargetBaseOrderBooks)
                {
                    var quoteTargetQuoteOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(quote.Id, target.Quote.Id)).ToList();
                    foreach (var quoteTargetQuoteOrderBook in quoteTargetQuoteOrderBooks)
                    {
                        if (!baseTargetBaseOrderBook.Asks.Any() && !baseTargetBaseOrderBook.Bids.Any()
                            || !woBaseAndQuoteOrderBook.Asks.Any() && !woBaseAndQuoteOrderBook.Bids.Any()
                            || !quoteTargetQuoteOrderBook.Asks.Any() && !quoteTargetQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(baseTargetBaseOrderBook, woBaseAndQuoteOrderBook, quoteTargetQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }

                // Trying to find pair from @base to target.Quote and quote to target.Base
                var baseTargetQuoteOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(@base.Id, target.Quote.Id)).ToList();
                foreach (var baseTargetQuoteOrderBook in baseTargetQuoteOrderBooks)
                {
                    var quoteTargetBaseOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(quote.Id, target.Base.Id)).ToList();
                    foreach (var quoteTargetBaseOrderBook in quoteTargetBaseOrderBooks)
                    {
                        if (!quoteTargetBaseOrderBook.Asks.Any() && !quoteTargetBaseOrderBook.Bids.Any()
                            || !woBaseAndQuoteOrderBook.Asks.Any() && !woBaseAndQuoteOrderBook.Bids.Any()
                            || !baseTargetQuoteOrderBook.Asks.Any() && !baseTargetQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(quoteTargetBaseOrderBook, woBaseAndQuoteOrderBook, baseTargetQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }
            }

            return result;
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFromAll(AssetPair target,
            IReadOnlyCollection<OrderBook> sourceOrderBooks, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            var synthOrderBookFrom1Pair = GetSynthsFrom1(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom1Pair);
            var synthOrderBookFrom2Pairs = GetSynthsFrom2(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom2Pairs);
            var synthOrderBookFrom3Pairs = GetSynthsFrom3(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom3Pairs);

            return result;
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFromAll(AssetPair target, IReadOnlyCollection<OrderBook> orderBooks)
        {
            return GetSynthsFromAll(target, orderBooks, orderBooks);
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFromAll(AssetPair target, OrderBook source,
            IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            return GetSynthsFromAll(target, new List<OrderBook> { source }, allOrderBooks);
        }


        public static IDictionary<AssetPair, OrderBook> PrepareForEnumeration(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
        {
            var result = new Dictionary<AssetPair, OrderBook>();

            var chainedAssetPairs = GetChained(orderBooks, target);
            var orderedOrderBooks = GetOrdered(orderBooks, target);
            Debug.Assert(orderBooks.Count == orderedOrderBooks.Count && orderedOrderBooks.Count == chainedAssetPairs.Count);

            for (var i = 0; i < orderBooks.Count; i++)
                result.Add(chainedAssetPairs[i], orderedOrderBooks[i]);

            return result;
        }

        public override string ToString()
        {
            return ConversionPath;
        }


        public IEnumerable<LimitOrder> GetPrices(Func<OrderBook, AssetPair, IEnumerable<LimitOrder>> method)
        {
            var prepared = PrepareForEnumeration(OriginalOrderBooks, AssetPair);

            if (prepared.Count == 1)
            {
                var keyValue = prepared.ElementAt(0);
                foreach (var limitOrder in method(keyValue.Value, keyValue.Key))
                    yield return limitOrder;
            }

            if (prepared.Count == 2)
            {
                var left = prepared.ElementAt(0);
                var right = prepared.ElementAt(1);

                foreach (var leftPrice in method(left.Value, left.Key))
                    foreach (var rightPrice in method(right.Value, right.Key))
                        yield return GetLimitOrder(leftPrice, rightPrice);
            }

            if (prepared.Count == 3)
            {
                var left = prepared.ElementAt(0);
                var middle = prepared.ElementAt(1);
                var right = prepared.ElementAt(2);

                foreach (var leftPrice in method(left.Value, left.Key))
                    foreach (var middlePrice in method(middle.Value, middle.Key))
                        foreach (var rightPrice in method(right.Value, right.Key))
                            yield return GetLimitOrder(leftPrice, middlePrice, rightPrice);
            }
        }

        private static IEnumerable<LimitOrder> GetBids(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(target != null && target.IsValid());
            Debug.Assert(target.EqualOrReversed(orderBook.AssetPair));

            // Streight
            if (orderBook.AssetPair.Base.Id == target.Base.Id &&
                orderBook.AssetPair.Quote.Id == target.Quote.Id)
            {
                for (var i = orderBook.Bids.Count - 1; i >= 0; i--)
                {
                    var bid = orderBook.Bids[i];
                    yield return bid;
                }
            }

            // Reversed
            if (orderBook.AssetPair.Base.Id == target.Quote.Id &&
                orderBook.AssetPair.Quote.Id == target.Base.Id)
            {
                foreach (var ask in orderBook.Asks)
                {
                    var bid = ask.Reciprocal();
                    yield return bid;
                }
            }
        }

        private static IEnumerable<LimitOrder> GetAsks(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(target != null && target.IsValid());
            Debug.Assert(target.EqualOrReversed(orderBook.AssetPair));

            // Streight
            if (orderBook.AssetPair.Base.Id == target.Base.Id &&
                orderBook.AssetPair.Quote.Id == target.Quote.Id)
            {
                foreach (var ask in orderBook.Asks)
                {
                    yield return ask;
                }
            }

            // Reversed
            if (orderBook.AssetPair.Base.Id == target.Quote.Id &&
                orderBook.AssetPair.Quote.Id == target.Base.Id)
            {
                for (var i = orderBook.Bids.Count - 1; i >= 0; i--)
                {
                    var bid = orderBook.Bids[i];
                    var ask = bid.Reciprocal();
                    yield return ask;
                }
            }
        }

        private static IReadOnlyList<OrderBook> GetOrdered(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
        {
            Debug.Assert(orderBooks != null);
            Debug.Assert(orderBooks.Any());
            Debug.Assert(target != null);
            Debug.Assert(target.IsValid());

            var result = new List<OrderBook>();

            var @base = target.Base;
            var quote = target.Quote;

            var first = orderBooks.Single(x => x.AssetPair.ContainsAsset(@base.Id));
            result.Add(first);

            if (orderBooks.Count == 1)
                return result;

            var nextAssetId = first.AssetPair.GetOtherAssetId(@base.Id);
            var second = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAssetId) && !x.AssetPair.EqualOrReversed(first.AssetPair));
            result.Add(second);

            if (orderBooks.Count == 2)
                return result;

            nextAssetId = second.AssetPair.GetOtherAssetId(nextAssetId);
            var third = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAssetId) && x.AssetPair.ContainsAsset(quote.Id));
            result.Add(third);

            return result;
        }

        private static IReadOnlyList<AssetPair> GetChained(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
        {
            Debug.Assert(orderBooks != null);
            Debug.Assert(orderBooks.Any());
            Debug.Assert(target != null);
            Debug.Assert(target.IsValid());

            var result = new List<AssetPair>();

            var @base = target.Base;
            var quote = target.Quote;

            var first = orderBooks.Single(x => x.AssetPair.ContainsAsset(@base.Id)).AssetPair;
            if (first.Quote.Id == @base.Id)
                first = first.Reverse();
            result.Add(first);

            if (orderBooks.Count == 1)
            {
                Debug.Assert(first.Quote.Id == quote.Id);
                return result;
            }

            var nextAssetId = first.GetOtherAssetId(@base.Id);
            var second = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAssetId) && !x.AssetPair.EqualOrReversed(first)).AssetPair;
            if (second.Quote.Id == nextAssetId)
                second = second.Reverse();
            result.Add(second);

            if (orderBooks.Count == 2)
            {
                Debug.Assert(second.Quote.Id == quote.Id);
                return result;
            }

            nextAssetId = second.GetOtherAssetId(nextAssetId);
            var third = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAssetId) && x.AssetPair.ContainsAsset(quote.Id)).AssetPair;
            if (third.Quote.Id == nextAssetId)
                third = third.Reverse();
            result.Add(third);

            Debug.Assert(third.Quote.Id == quote.Id);
            return result;
        }

        private static LimitOrder GetLimitOrder(LimitOrder left, LimitOrder right)
        {
            var newBidPrice = left.Price * right.Price;
            var rightBidVolumeInBaseAsset = right.Volume / left.Price;
            var newBidVolume = Math.Min(left.Volume, rightBidVolumeInBaseAsset);

            var result = new LimitOrder(newBidPrice, newBidVolume);

            return result;
        }

        private static LimitOrder GetLimitOrder(LimitOrder left, LimitOrder middle, LimitOrder right)
        {
            var newBidPrice = left.Price * middle.Price * right.Price;
            var interimBidPrice = left.Price * middle.Price;
            var interimBidVolumeInBaseAsset = middle.Volume / left.Price;
            var rightBidVolumeInBaseAsset = right.Volume / interimBidPrice;
            var newBidVolume = Math.Min(Math.Min(left.Volume, interimBidVolumeInBaseAsset), rightBidVolumeInBaseAsset);

            var result = new LimitOrder(newBidPrice, newBidVolume);

            return result;
        }
    }
}

