using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class SynthOrderBookTests
    {
        private readonly AssetPair _btcusd = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
        private readonly AssetPair _btceur = new AssetPair("BTCEUR", "BTCEUR", new Asset("BTC", "BTC"), new Asset("EUR", "EUR"));
        private readonly AssetPair _eurusd = new AssetPair("EURUSD", "EURUSD", new Asset("EUR", "EUR"), new Asset("USD", "USD"));
        private readonly AssetPair _eurchf = new AssetPair("EURCHF", "EURCHF", new Asset("EUR", "EUR"), new Asset("CHF", "CHF"));
        private readonly AssetPair _chfusd = new AssetPair("CHFUSD", "CHFUSD", new Asset("CHF", "CHF"), new Asset("USD", "USD"));

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_Test()
        {
            var orderBook = new OrderBook("FE", _btcusd, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            var result = SynthOrderBook.PrepareForEnumeration(new List<OrderBook> { orderBook }, _btcusd);

            Assert.Single(result);
            Assert.True(result.Single().Key.Equals(_btcusd));
            Assert.True(result.Single().Value.AssetPair.EqualOrReversed(_btcusd));
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_Test()
        {
            var usdbtc = _btcusd.Reverse();
            var orderBook = new OrderBook("FE", usdbtc, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);

            var result = SynthOrderBook.PrepareForEnumeration(new List<OrderBook> { orderBook }, _btcusd);

            Assert.Single(result);
            Assert.True(result.Single().Key.Equals(_btcusd));
            Assert.True(result.Single().Value.AssetPair.EqualOrReversed(_btcusd));
        }


        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }


        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_0_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf, _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_0_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf, _chfusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf.Reverse(), _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurchf, _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf.Reverse(), _chfusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurchf.Reverse(), _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurchf, _chfusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Reverse(), _eurchf.Reverse(), _chfusd.Reverse());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }


        private IReadOnlyCollection<OrderBook> GetOrderBooks(AssetPair assetPair1, AssetPair assetPair2)
        {
            var orderBook1 = new OrderBook("FE", assetPair1, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            orderBook1.SetAssetPair(assetPair1);
            var orderBook2 = new OrderBook("FE", assetPair2, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            orderBook2.SetAssetPair(assetPair2);

            return new List<OrderBook> { orderBook1, orderBook2 };
        }

        private IReadOnlyCollection<OrderBook> GetOrderBooks(AssetPair assetPair1, AssetPair assetPair2, AssetPair assetPair3)
        {
            var orderBook1 = new OrderBook("FE", assetPair1, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            orderBook1.SetAssetPair(assetPair1);
            var orderBook2 = new OrderBook("FE", assetPair2, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            orderBook2.SetAssetPair(assetPair2);
            var orderBook3 = new OrderBook("FE", assetPair3, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            orderBook3.SetAssetPair(assetPair3);

            return new List<OrderBook> { orderBook1, orderBook2, orderBook3 };
        }

        private void AssertChained2(IDictionary<AssetPair, OrderBook> result)
        {
            Assert.Equal(2, result.Count);

            Assert.Equal("BTC", result.ElementAt(0).Key.Base.Id);
            Assert.Equal("EUR", result.ElementAt(0).Key.Quote.Id);
            Assert.True(result.ElementAt(0).Value.AssetPair.EqualOrReversed(_btceur));

            Assert.Equal("EUR", result.ElementAt(1).Key.Base.Id);
            Assert.Equal("USD", result.ElementAt(1).Key.Quote.Id);
            Assert.True(result.ElementAt(1).Value.AssetPair.EqualOrReversed(_eurusd));
        }

        private void AssertChained3(IDictionary<AssetPair, OrderBook> result)
        {
            Assert.Equal(3, result.Count);

            Assert.Equal("BTC", result.ElementAt(0).Key.Base.Id);
            Assert.Equal("EUR", result.ElementAt(0).Key.Quote.Id);
            Assert.True(result.ElementAt(0).Value.AssetPair.EqualOrReversed(_btceur));

            Assert.Equal("EUR", result.ElementAt(1).Key.Base.Id);
            Assert.Equal("CHF", result.ElementAt(1).Key.Quote.Id);
            Assert.True(result.ElementAt(1).Value.AssetPair.EqualOrReversed(_eurchf));

            Assert.Equal("CHF", result.ElementAt(2).Key.Base.Id);
            Assert.Equal("USD", result.ElementAt(2).Key.Quote.Id);
            Assert.True(result.ElementAt(2).Value.AssetPair.EqualOrReversed(_chfusd));
        }
    }
}
