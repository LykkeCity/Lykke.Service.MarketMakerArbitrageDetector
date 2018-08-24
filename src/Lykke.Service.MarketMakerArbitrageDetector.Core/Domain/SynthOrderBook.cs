using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class SynthOrderBook : OrderBook
    {
        public string ConversionPath { get; }

        public IReadOnlyCollection<OrderBook> OriginalOrderBooks { get; }

        public SynthOrderBook(string exchange, AssetPair assetPair, IReadOnlyCollection<LimitOrder> bids, IReadOnlyCollection<LimitOrder> asks,
            string conversionPath, IReadOnlyCollection<OrderBook> originalOrderBooks, DateTime timestamp)
            : base(exchange, assetPair, bids, asks, timestamp)
        {
            ConversionPath = conversionPath;
            OriginalOrderBooks = originalOrderBooks;
        }


        public static SynthOrderBook FromOrderBook(OrderBook orderBook, AssetPair targetAssetPair)
        {
            #region Checking arguments

            if (orderBook == null)
                throw new ArgumentNullException(nameof(orderBook));

            if (targetAssetPair == null || !targetAssetPair.IsValid())
                throw new ArgumentException(nameof(targetAssetPair));

            if (!targetAssetPair.IsEqualOrReversed(orderBook.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(orderBook.AssetPair)} and {nameof(targetAssetPair)} aren't semantically equal.");

            #endregion

            var originalOrderBooks = new List<OrderBook>();
            OrderBook orderBookResult = null;

            // Streight
            if (orderBook.AssetPair.Base.Id == targetAssetPair.Base.Id && orderBook.AssetPair.Quote.Id == targetAssetPair.Quote.Id)
            {
                orderBookResult = orderBook;
                originalOrderBooks.Add(orderBook);
            }

            // Reversed
            if (orderBook.AssetPair.Base.Id == targetAssetPair.Quote.Id && orderBook.AssetPair.Quote.Id == targetAssetPair.Base.Id)
            {
                orderBookResult = orderBook.Reverse();
                originalOrderBooks.Add(orderBook);
            }

            if (orderBookResult == null)
                throw new InvalidOperationException("AssetPairs must be the same or reversed)");

            var conversionPath = orderBook.AssetPair.ToString();
            var result = new SynthOrderBook (
                orderBookResult.Exchange,
                targetAssetPair,
                orderBookResult.Bids,
                orderBookResult.Asks,
                conversionPath,
                originalOrderBooks,
                orderBook.Timestamp
            );

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook one, OrderBook another, AssetPair targetAssetPair)
        {
            #region Checking arguments 

            if (one == null)
                throw new ArgumentNullException(nameof(one));

            if (another == null)
                throw new ArgumentNullException(nameof(another));

            if (one.AssetPair == null || !one.AssetPair.IsValid())
                throw new ArgumentException(nameof(one) + "." + nameof(one.AssetPair));

            if (another.AssetPair == null || !another.AssetPair.IsValid())
                throw new ArgumentException(nameof(another) + "." + nameof(another.AssetPair));

            if (!targetAssetPair.IsValid())
                throw new ArgumentException(nameof(targetAssetPair));

            #endregion

            // Prepare left and right order books for calculating target
            OrderBook left = null;
            OrderBook right = null;

            if (targetAssetPair.Base.Id == one.AssetPair.Base.Id)
                left = one;

            if (targetAssetPair.Base.Id == one.AssetPair.Quote.Id)
                left = one.Reverse();

            if (targetAssetPair.Base.Id == another.AssetPair.Base.Id)
                left = another;

            if (targetAssetPair.Base.Id == another.AssetPair.Quote.Id)
                left = another.Reverse();

            if (targetAssetPair.Quote.Id == one.AssetPair.Base.Id)
                right = one.Reverse();

            if (targetAssetPair.Quote.Id == one.AssetPair.Quote.Id)
                right = one;

            if (targetAssetPair.Quote.Id == another.AssetPair.Base.Id)
                right = another.Reverse();

            if (targetAssetPair.Quote.Id == another.AssetPair.Quote.Id)
                right = another;

            #region Checking left and right

            if (left == null || right == null)
                throw new ArgumentException($"Order books don't correspond to {targetAssetPair}");

            if (left.AssetPair.Base.Id != targetAssetPair.Base.Id
                || right.AssetPair.Quote.Id != targetAssetPair.Quote.Id
                || left.AssetPair.Quote.Id != right.AssetPair.Base.Id)
                throw new ArgumentException($"Order books don't correspond to {targetAssetPair}");

            #endregion

            var bids = new List<LimitOrder>();
            var asks = new List<LimitOrder>();

            // Calculating new bids
            foreach (var leftBid in left.Bids)
            {
                foreach (var rightBid in right.Bids)
                {
                    var newBidPrice = Math.Round(leftBid.Price * rightBid.Price, 6);
                    var rightBidVolumeInBaseAsset = Math.Round(rightBid.Volume / leftBid.Price, 6);
                    var newBidVolume = Math.Min(leftBid.Volume, rightBidVolumeInBaseAsset);

                    var newClientId = $"{leftBid.ClientId} + {rightBid.ClientId}";
                    var newOrderId = $"{leftBid.OrderId} + {rightBid.OrderId}";
                    var newBidVolumePrice = new LimitOrder(newOrderId, newClientId, newBidPrice, newBidVolume);
                    bids.Add(newBidVolumePrice);
                }
            }

            // Calculating new asks
            foreach (var leftAsk in left.Asks)
            {
                foreach (var rightAsk in right.Asks)
                {
                    var newAskPrice = Math.Round(leftAsk.Price * rightAsk.Price, 6);
                    var rightAskVolumeInBaseAsset = Math.Round(rightAsk.Volume / leftAsk.Price, 6);
                    var newAskVolume = Math.Min(leftAsk.Volume, rightAskVolumeInBaseAsset);

                    var newClientId = $"{leftAsk.ClientId} + {rightAsk.ClientId}";
                    var newOrderId = $"{leftAsk.OrderId} + {rightAsk.OrderId}";
                    var newAskVolumePrice = new LimitOrder(newOrderId, newClientId, newAskPrice, newAskVolume);
                    asks.Add(newAskVolumePrice);
                }
            }

            var exchange = GetSourcesPath(one.Exchange, another.Exchange);
            var conversionPath = GetConversionPath(one, another);
            var originalOrderBooks = new List<OrderBook> { one, another };
            var timestamp = left.Timestamp < right.Timestamp ? left.Timestamp : right.Timestamp;

            var result = new SynthOrderBook(exchange, targetAssetPair, bids, asks, conversionPath, originalOrderBooks, timestamp);

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook one, OrderBook second, OrderBook third, AssetPair targetAssetPair)
        {
            #region Checking arguments 

            if (one == null)
                throw new ArgumentNullException(nameof(one));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            if (third == null)
                throw new ArgumentNullException(nameof(third));

            if (!one.AssetPair.IsValid())
                throw new ArgumentException(nameof(one) + "." + nameof(one.AssetPair));

            if (!second.AssetPair.IsValid())
                throw new ArgumentException(nameof(second) + "." + nameof(second.AssetPair));

            if (!third.AssetPair.IsValid())
                throw new ArgumentException(nameof(third) + "." + nameof(third.AssetPair));

            if (!targetAssetPair.IsValid())
                throw new ArgumentException(nameof(targetAssetPair));

            #endregion

            var orderBooks = new List<OrderBook> { one, second, third };

            OrderBook FindOrderBookByAssetId(string assetId)
            {
                foreach (var orderBook in orderBooks)
                {
                    if (orderBook.AssetPair.ContainsAsset(assetId))
                    {
                        orderBooks.Remove(orderBook);
                        return orderBook;
                    }
                }

                throw new InvalidOperationException("This point can't be reached");
            }

            // Prepare left, middle and right order books for calculating target

            var left = FindOrderBookByAssetId(targetAssetPair.Base.Id);
            if (left == null)
                throw new ArgumentException("There is no asset pair with target.Base asset");
            if (left.AssetPair.Quote.Id == targetAssetPair.Base.Id)
                left = left.Reverse();

            var intermediate1Asset = left.AssetPair.Quote;
            var middle = FindOrderBookByAssetId(intermediate1Asset.Id);
            if (middle == null)
                throw new ArgumentException("There is no asset pair with intermediate1 asset");
            if (middle.AssetPair.Quote.Id == intermediate1Asset.Id)
                middle = middle.Reverse();

            var intermediate2Asset = middle.AssetPair.Quote;
            var right = FindOrderBookByAssetId(intermediate2Asset.Id);
            if (right == null)
                throw new ArgumentException("There is no asset pair with intermediate2 asset");
            if (right.AssetPair.Quote.Id == intermediate2Asset.Id)
                right = right.Reverse();

            #region Checking left, middle and right

            if (left.AssetPair.Base.Id != targetAssetPair.Base.Id
                || left.AssetPair.Quote.Id != middle.AssetPair.Base.Id
                || middle.AssetPair.Quote.Id != right.AssetPair.Base.Id
                || right.AssetPair.Quote.Id != targetAssetPair.Quote.Id)
                throw new ArgumentException($"Order books don't correspond to {targetAssetPair}");

            #endregion

            var bids = new List<LimitOrder>();
            var asks = new List<LimitOrder>();

            // Calculating new bids
            foreach (var leftBid in left.Bids)
            {
                foreach (var middleBid in middle.Bids)
                {
                    foreach (var rightBid in right.Bids)
                    {
                        var newBidPrice = Math.Round(leftBid.Price * middleBid.Price * rightBid.Price, 6);
                        var interimBidPrice = Math.Round(leftBid.Price * middleBid.Price, 6);
                        var interimBidVolumeInBaseAsset = Math.Round(middleBid.Volume / leftBid.Price, 6);
                        var rightBidVolumeInBaseAsset = Math.Round(rightBid.Volume / interimBidPrice, 6);
                        var newBidVolume = Math.Min(Math.Min(leftBid.Volume, interimBidVolumeInBaseAsset), rightBidVolumeInBaseAsset);

                        var newClientId = $"{leftBid.ClientId} + {middleBid.ClientId} + {rightBid.ClientId}";
                        var newOrderId = $"{leftBid.OrderId} + {middleBid.OrderId} + {rightBid.OrderId}";
                        var newBidVolumePrice = new LimitOrder(newOrderId, newClientId, newBidPrice, newBidVolume);
                        bids.Add(newBidVolumePrice);
                    }
                }
            }

            // Calculating new asks
            foreach (var leftAsk in left.Asks)
            {
                foreach (var middleAsk in middle.Asks)
                {
                    foreach (var rightAsk in right.Asks)
                    {
                        var newAskPrice = Math.Round(leftAsk.Price * middleAsk.Price * rightAsk.Price, 6);
                        var interimAskPrice = Math.Round(leftAsk.Price * middleAsk.Price, 6);
                        var interimAskVolumeInBaseAsset = Math.Round(middleAsk.Volume / leftAsk.Price, 6);
                        var rightAskVolumeInBaseAsset = Math.Round(rightAsk.Volume / interimAskPrice, 6);
                        var newAskVolume = Math.Min(Math.Min(leftAsk.Volume, interimAskVolumeInBaseAsset), rightAskVolumeInBaseAsset);

                        var newClientId = $"{leftAsk.ClientId} + {middleAsk.ClientId} + {rightAsk.ClientId}";
                        var newOrderId = $"{leftAsk.OrderId} + {middleAsk.OrderId} + {rightAsk.OrderId}";
                        var newAskVolumePrice = new LimitOrder(newOrderId, newClientId, newAskPrice, newAskVolume);
                        asks.Add(newAskVolumePrice);
                    }
                }
            }

            var exchange = GetSourcesPath(one.Exchange, second.Exchange, third.Exchange);
            var conversionPath = GetConversionPath(one, second, third);
            var originalOrderBooks = new List<OrderBook> { one, second, third };

            var interimTimestamp = left.Timestamp < middle.Timestamp ? left.Timestamp : middle.Timestamp;
            var timestamp = interimTimestamp < right.Timestamp ? interimTimestamp : right.Timestamp;

            var result = new SynthOrderBook(exchange, targetAssetPair, bids, asks, conversionPath, originalOrderBooks, timestamp);

            return result;
        }


        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom1(AssetPair target, IReadOnlyCollection<OrderBook> orderBooks)
        {
            return GetSynthsFrom1(target, orderBooks, orderBooks);
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom1(AssetPair target, OrderBook source, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            return GetSynthsFrom1(target, new List<OrderBook> { source }, allOrderBooks);
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
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAsset(target.Base.Id)
                                  ?? withBaseOrQuoteAssetPair.GetOtherAsset(target.Quote.Id);

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


        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom2(AssetPair target, IReadOnlyCollection<OrderBook> orderBooks)
        {
            return GetSynthsFrom2(target, orderBooks, orderBooks);
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom2(AssetPair target,
            OrderBook source, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            return GetSynthsFrom2(target, new List<OrderBook> { source }, allOrderBooks);
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
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAsset(target.Base.Id)
                                ?? withBaseOrQuoteAssetPair.GetOtherAsset(target.Quote.Id);

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


        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom3(AssetPair target, IReadOnlyCollection<OrderBook> orderBooks)
        {
            return GetSynthsFrom3(target, orderBooks, orderBooks);
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFrom3(AssetPair target,
            OrderBook source, IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            return GetSynthsFrom3(target, new List<OrderBook> { source }, allOrderBooks);
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


        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFromAll(AssetPair target, IReadOnlyCollection<OrderBook> orderBooks)
        {
            return GetSynthsFromAll(target, orderBooks, orderBooks);
        }

        public static IReadOnlyCollection<SynthOrderBook> GetSynthsFromAll(AssetPair target, OrderBook source,
            IReadOnlyCollection<OrderBook> allOrderBooks)
        {
            return GetSynthsFromAll(target, new List<OrderBook> { source }, allOrderBooks);
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


        public static string GetConversionPath(OrderBook left, OrderBook right)
        {
            return $"{left.AssetPair} & {right.AssetPair}";
        }

        public static string GetConversionPath(OrderBook left, OrderBook middle, OrderBook right)
        {
            return $"{left.AssetPair} & {middle.AssetPair} & {right.AssetPair}";
        }

        public static string GetSourcesPath(string leftSource, string rightSource)
        {
            return $"{leftSource} - {rightSource}";
        }

        public static string GetSourcesPath(string leftSource, string middleSource, string rightSource)
        {
            return $"{leftSource} - {middleSource} - {rightSource}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}

