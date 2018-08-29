using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class SynthOrderBook
    {
        public string Exchange => string.Join(" - ", OriginalOrderBooks.Select(x => x.Exchange));

        public AssetPair AssetPair { get; }

        public IEnumerable<LimitOrder> Bids => GetPrices(GetBids, OrderByDirection.Descending);

        public IEnumerable<LimitOrder> Asks => GetPrices(GetAsks, OrderByDirection.Ascending);

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


        public IEnumerable<LimitOrder> GetPrices(Func<OrderBook, AssetPair, IEnumerable<LimitOrder>> method, OrderByDirection direction)
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

                var leftPricesEnumerator = method(left.Value, left.Key).GetEnumerator();
                var rightPricesEnumerator = method(right.Value, right.Key).GetEnumerator();

                if (!leftPricesEnumerator.MoveNext() || !rightPricesEnumerator.MoveNext())
                    yield break;

                var leftCurrentOrders = new List<LimitOrder>();
                var rightCurrentOrders = new List<LimitOrder>();

                var newLeftOrder = leftPricesEnumerator.Current.CloneWithoutIds();
                var newRightOrder = rightPricesEnumerator.Current.CloneWithoutIds();

                leftCurrentOrders.Add(newLeftOrder);
                rightCurrentOrders.Add(newRightOrder);

                // Just return first generated order
                yield return SynthLimitOrder(newLeftOrder, newRightOrder);

                while (true)
                {
                    newLeftOrder = leftPricesEnumerator.MoveNext() ? leftPricesEnumerator.Current.CloneWithoutIds() : null;
                    newRightOrder = rightPricesEnumerator.MoveNext() ? rightPricesEnumerator.Current.CloneWithoutIds() : null;

                    // TODO: Can be rewritten without neddless call to MoveNext() while it is already null.
                    if (newLeftOrder == null && newRightOrder == null)
                        break;

                    var newOrdersWithNewLeftOrder = new List<LimitOrder>();
                    if (newLeftOrder != null)
                        foreach (var rightCurrentOrder in rightCurrentOrders)
                            newOrdersWithNewLeftOrder.Add(SynthLimitOrder(newLeftOrder, rightCurrentOrder));

                    var newOrdersWithNewRightOrder = new List<LimitOrder>();
                    if (newRightOrder != null)
                        foreach (var leftCurrentOrder in leftCurrentOrders)
                            newOrdersWithNewRightOrder.Add(SynthLimitOrder(leftCurrentOrder, newRightOrder));

                    var newCurrentLevelOrders = new List<LimitOrder>();
                    newCurrentLevelOrders.AddRange(newOrdersWithNewLeftOrder);
                    newCurrentLevelOrders.AddRange(newOrdersWithNewRightOrder);
                    if (newLeftOrder != null && newRightOrder != null)
                        newCurrentLevelOrders.Add(SynthLimitOrder(newLeftOrder, newRightOrder));
                    var newOrders = newCurrentLevelOrders.OrderBy(x => x.Price, direction);

                    foreach (var limitOrder in newOrders)
                        yield return limitOrder;

                    if (newLeftOrder != null)
                        leftCurrentOrders.Add(newLeftOrder);

                    if (newRightOrder != null)
                        rightCurrentOrders.Add(newRightOrder);
                }

                leftPricesEnumerator.Dispose();
                rightPricesEnumerator.Dispose();
            }

            if (prepared.Count == 3)
            {
                var left = prepared.ElementAt(0);
                var middle = prepared.ElementAt(1);
                var right = prepared.ElementAt(2);

                var leftPricesEnumerator = method(left.Value, left.Key).GetEnumerator();
                var middlePricesEnumerator = method(middle.Value, middle.Key).GetEnumerator();
                var rightPricesEnumerator = method(right.Value, right.Key).GetEnumerator();

                if (!leftPricesEnumerator.MoveNext() || !middlePricesEnumerator.MoveNext() || !rightPricesEnumerator.MoveNext())
                    yield break;

                var leftCurrentOrders = new List<LimitOrder>();
                var middleCurrentOrders = new List<LimitOrder>();
                var rightCurrentOrders = new List<LimitOrder>();

                var newLeftOrder = leftPricesEnumerator.Current.CloneWithoutIds();
                var newMiddleOrder = middlePricesEnumerator.Current.CloneWithoutIds();
                var newRightOrder = rightPricesEnumerator.Current.CloneWithoutIds();

                leftCurrentOrders.Add(newLeftOrder);
                middleCurrentOrders.Add(newMiddleOrder);
                rightCurrentOrders.Add(newRightOrder);

                // Just return first generated order
                yield return SynthLimitOrder(newLeftOrder, newMiddleOrder, newRightOrder);

                while (true)
                {
                    newLeftOrder = leftPricesEnumerator.MoveNext() ? leftPricesEnumerator.Current.CloneWithoutIds() : null;
                    newMiddleOrder = middlePricesEnumerator.MoveNext() ? middlePricesEnumerator.Current.CloneWithoutIds() : null;
                    newRightOrder = rightPricesEnumerator.MoveNext() ? rightPricesEnumerator.Current.CloneWithoutIds() : null;

                    if (newLeftOrder == null && newMiddleOrder == null && newRightOrder == null)
                        break;

                    var newCurrentLevelOrders = new List<LimitOrder>();

                    var newOrdersWithNewLeftOrder = new List<LimitOrder>();
                    if (newLeftOrder != null)
                        foreach (var middleCurrentOrder in newMiddleOrder == null ? middleCurrentOrders : middleCurrentOrders.Concat(newMiddleOrder))
                            foreach (var rightCurrentOrder in rightCurrentOrders)
                                newOrdersWithNewLeftOrder.Add(SynthLimitOrder(newLeftOrder, middleCurrentOrder, rightCurrentOrder));

                    var newOrdersWithNewMiddleOrder = new List<LimitOrder>();
                    if (newMiddleOrder != null)
                        foreach (var leftCurrentOrder in leftCurrentOrders)
                            foreach (var rightCurrentOrder in rightCurrentOrders)
                                newOrdersWithNewMiddleOrder.Add(SynthLimitOrder(leftCurrentOrder, newMiddleOrder, rightCurrentOrder));

                    var newOrdersWithAllMiddleOrders = new List<LimitOrder>();
                    if (newLeftOrder != null && newRightOrder != null)
                        foreach (var middleCurrentOrder in newMiddleOrder == null ? middleCurrentOrders : middleCurrentOrders.Concat(newMiddleOrder))
                            newOrdersWithAllMiddleOrders.Add(SynthLimitOrder(newLeftOrder, middleCurrentOrder, newRightOrder));

                    var newOrdersWithNewRightOrder = new List<LimitOrder>();
                    if (newRightOrder != null)
                        foreach (var leftCurrentOrder in leftCurrentOrders)
                            foreach (var middleCurrentOrder in newMiddleOrder == null ? middleCurrentOrders : middleCurrentOrders.Concat(newMiddleOrder))
                                newOrdersWithNewRightOrder.Add(SynthLimitOrder(leftCurrentOrder, middleCurrentOrder, newRightOrder));

                    newCurrentLevelOrders.AddRange(newOrdersWithNewLeftOrder);
                    newCurrentLevelOrders.AddRange(newOrdersWithNewMiddleOrder);
                    newCurrentLevelOrders.AddRange(newOrdersWithAllMiddleOrders);
                    newCurrentLevelOrders.AddRange(newOrdersWithNewRightOrder);
                    var newOrders = newCurrentLevelOrders.OrderBy(x => x.Price, direction);
                    
                    foreach (var limitOrder in newOrders)
                        yield return limitOrder;

                    if (newLeftOrder != null)
                        leftCurrentOrders.Add(newLeftOrder);

                    if (newMiddleOrder != null)
                        middleCurrentOrders.Add(newMiddleOrder);

                    if (newRightOrder != null)
                        rightCurrentOrders.Add(newRightOrder);
                }

                leftPricesEnumerator.Dispose();
                middlePricesEnumerator.Dispose();
                rightPricesEnumerator.Dispose();
            }
        }

        public static IEnumerable<LimitOrder> GetBids(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(target != null && target.IsValid());
            Debug.Assert(target.EqualOrReversed(orderBook.AssetPair));

            var bids = orderBook.Bids;
            var asks = orderBook.Asks;

            // Streight
            if (orderBook.AssetPair.Base.Id == target.Base.Id &&
                orderBook.AssetPair.Quote.Id == target.Quote.Id)
            {
                foreach (var bid in bids)
                {
                    yield return bid;
                }
            }

            // Reversed
            if (orderBook.AssetPair.Base.Id == target.Quote.Id &&
                orderBook.AssetPair.Quote.Id == target.Base.Id)
            {
                foreach (var ask in asks)
                {
                    var bid = ask.Reciprocal();
                    yield return bid;
                }
            }
        }

        public static IEnumerable<LimitOrder> GetAsks(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(target != null && target.IsValid());
            Debug.Assert(target.EqualOrReversed(orderBook.AssetPair));

            var bids = orderBook.Bids;
            var asks = orderBook.Asks;

            // Streight
            if (orderBook.AssetPair.Base.Id == target.Base.Id &&
                orderBook.AssetPair.Quote.Id == target.Quote.Id)
            {
                foreach (var ask in asks)
                {
                    yield return ask;
                }
            }

            // Reversed
            if (orderBook.AssetPair.Base.Id == target.Quote.Id &&
                orderBook.AssetPair.Quote.Id == target.Base.Id)
            {
                foreach (var bid in bids)
                {
                    var ask = bid.Reciprocal();
                    yield return ask;
                }
            }
        }

        public static IReadOnlyList<OrderBook> GetOrdered(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
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

        public static IReadOnlyList<AssetPair> GetChained(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
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

        private static LimitOrder SynthLimitOrder(LimitOrder left, LimitOrder right)
        {
            var newPrice = left.Price * right.Price;
            var rightBidVolumeInBaseAsset = right.Volume / left.Price;
            var newVolume = Math.Min(left.Volume, rightBidVolumeInBaseAsset);

            var result = new LimitOrder(newPrice, newVolume);

            return result;
        }

        private static LimitOrder SynthLimitOrder(LimitOrder left, LimitOrder middle, LimitOrder right)
        {
            var newPrice = left.Price * middle.Price * right.Price;
            var interimBidPrice = left.Price * middle.Price;
            var interimBidVolumeInBaseAsset = middle.Volume / left.Price;
            var rightBidVolumeInBaseAsset = right.Volume / interimBidPrice;
            var newVolume = Math.Min(Math.Min(left.Volume, interimBidVolumeInBaseAsset), rightBidVolumeInBaseAsset);

            var result = new LimitOrder(newPrice, newVolume);

            return result;
        }
    }
}

