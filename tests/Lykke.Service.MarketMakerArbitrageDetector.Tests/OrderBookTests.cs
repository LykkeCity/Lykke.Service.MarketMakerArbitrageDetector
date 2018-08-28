using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class OrderBookTests
    {
        private readonly AssetPair _btcusd = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));

        [Fact]
        public void SetAssetPairTest()
        {
            const string exchangeName = "FakeExchange";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(exchangeName, new AssetPair("BTCUSD"), 
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
        public void ReverseTest()
        {
            const string exchangeName = "FakeExchange";
            const string reversedPair = "USDBTC";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(exchangeName, _btcusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9), new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10), new LimitOrder(8999.95m, 7), new LimitOrder(8900.12345677m, 3)
                },
                timestamp);

            var reversed = orderBook.Reverse();
            Assert.NotNull(reversed);
            Assert.Equal(exchangeName, reversed.Exchange);
            Assert.Equal(reversedPair, reversed.AssetPair.Name);
            Assert.Equal(orderBook.Bids.Count, reversed.Asks.Count);
            Assert.Equal(orderBook.Asks.Count, reversed.Bids.Count);

            var bidLimitOrder1 = reversed.Bids.Single(x => x.Volume == 26700.37037031m);
            var bidLimitOrder2 = reversed.Bids.Single(x => x.Volume == 62999.65m);
            var bidLimitOrder3 = reversed.Bids.Single(x => x.Volume == 90000);
            Assert.Equal(bidLimitOrder1.Price, 1 / 8900.12345677m, 8);
            Assert.Equal(bidLimitOrder2.Price, 1 / 8999.95m, 8);
            Assert.Equal(bidLimitOrder3.Price, 1 / 9000m, 8);

            var askLimitOrder1 = reversed.Asks.Single(x => x.Volume == 79425);
            var askLimitOrder2 = reversed.Asks.Single(x => x.Volume == 44115);
            Assert.Equal(askLimitOrder1.Price, 1 / 8825m, 8);
            Assert.Equal(askLimitOrder2.Price, 1 / 8823m, 8);

            Assert.Equal(timestamp, reversed.Timestamp);
        }
    }
}
