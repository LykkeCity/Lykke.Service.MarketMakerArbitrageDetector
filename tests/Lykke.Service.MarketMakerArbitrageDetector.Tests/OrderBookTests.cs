using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class OrderBookTests
    {
        private readonly AssetPair _btcusd = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"), 8, 8);

        [Fact]
        public void SetAssetPairTest()
        {
            const string source = "FakeExchange";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(source, new AssetPair("BTCUSD"), 
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9), new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10), new LimitOrder(8999.95m, 7), new LimitOrder(8900.12345677m, 3)
                },
                timestamp);

            Assert.Null(orderBook.AssetPair.Base);
            Assert.Null(orderBook.AssetPair.Quote);
            orderBook.SetAssetPair(_btcusd);
            Assert.Equal("BTC", orderBook.AssetPair.Base.Id);
            Assert.Equal("USD", orderBook.AssetPair.Quote.Id);
            Assert.Equal(_btcusd, orderBook.AssetPair);
        }

        [Fact]
        public void InvertTest()
        {
            const string source = "FakeExchange";
            const string invertedPair = "USDBTC";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(source, _btcusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9), new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10), new LimitOrder(8999.95m, 7), new LimitOrder(8900.12345677m, 3)
                },
                timestamp);

            var inverted = orderBook.Invert();
            Assert.NotNull(inverted);
            Assert.Equal(source, inverted.Source);
            Assert.Equal(invertedPair, inverted.AssetPair.Name);
            Assert.Equal(orderBook.Bids.Count(), inverted.Asks.Count());
            Assert.Equal(orderBook.Asks.Count(), inverted.Bids.Count());

            var bidLimitOrder1 = inverted.Bids.Single(x => x.Volume == 26700.37037031m);
            var bidLimitOrder2 = inverted.Bids.Single(x => x.Volume == 62999.65m);
            var bidLimitOrder3 = inverted.Bids.Single(x => x.Volume == 90000);
            Assert.Equal(bidLimitOrder1.Price, 1 / 8900.12345677m, 8);
            Assert.Equal(bidLimitOrder2.Price, 1 / 8999.95m, 8);
            Assert.Equal(bidLimitOrder3.Price, 1 / 9000m, 8);

            var askLimitOrder1 = inverted.Asks.Single(x => x.Volume == 79425);
            var askLimitOrder2 = inverted.Asks.Single(x => x.Volume == 44115);
            Assert.Equal(askLimitOrder1.Price, 1 / 8825m, 8);
            Assert.Equal(askLimitOrder2.Price, 1 / 8823m, 8);

            Assert.Equal(timestamp, inverted.Timestamp);
        }
    }
}
