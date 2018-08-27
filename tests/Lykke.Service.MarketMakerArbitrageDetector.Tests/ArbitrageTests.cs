using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class ArbitrageTests
    {
        [Fact]
        public void ArbitrageVolume_NoArbitrage_EmptyOrderBooks_Test()
        {
            const string exchangeName = "FE";
            var assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var timestamp = DateTime.UtcNow;

            var orderBook1 = new OrderBook(exchangeName, assetPair, new List<LimitOrder>(), new List<LimitOrder>(), timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, new List<LimitOrder>(), new List<LimitOrder>(), timestamp);

            var volume = Arbitrage.GetArbitrageVolumePnL(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_NoArbitrage_TheSameOrderBook_Test()
        {
            const string exchangeName = "FE";
            var assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var timestamp = DateTime.UtcNow;

            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 10),
                new LimitOrder("", "", 8999.95m, 7),
                new LimitOrder("", "", 8900.12345677m, 3)
            };
            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 8825, 9),
                new LimitOrder("", "", 8823, 5)
            };

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolumePnL(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolumePnL_Simple1_Test()
        {
            const string exchangeName = "FE";
            var assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var timestamp = DateTime.UtcNow;

            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 10),
                new LimitOrder("", "", 8999.95m, 7), // <-
                new LimitOrder("", "", 8900.12345677m, 3) // <-
            };

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 9), // <-
                new LimitOrder("", "", 8900, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumePnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(9, volumePnL?.Volume);
            Assert.Equal(299.92962969m, volumePnL?.PnL);
        }

        [Fact]
        public void ArbitrageVolumePnL_Complex1_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=0

            const string exchangeName = "FE";
            var assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var timestamp = DateTime.UtcNow;

            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 1000, 10),
                new LimitOrder("", "", 950, 10),
                new LimitOrder("", "", 850, 10), // <-
                new LimitOrder("", "", 800, 10), // <-
                new LimitOrder("", "", 700, 10), // <-
                new LimitOrder("", "", 650, 10), // <-
                new LimitOrder("", "", 600, 10), // <-
                new LimitOrder("", "", 550, 1),  // <-
                new LimitOrder("", "", 500, 10)  // <-
            };

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 900, 5),   // <-
                new LimitOrder("", "", 750, 100), // <-
                new LimitOrder("", "", 550, 1)    // <-
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumePnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(41, volumePnL?.Volume);
            Assert.Equal(6450, volumePnL?.PnL);
        }

        [Fact]
        public void ArbitrageVolumePnL_Complex2_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=2011486790
            const string exchangeName = "FE";
            var assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var timestamp = DateTime.UtcNow;

            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 2.9m, 10),
                new LimitOrder("", "", 2.8m, 10),
                new LimitOrder("", "", 2.7m, 10),
                new LimitOrder("", "", 2.6m, 10),
                new LimitOrder("", "", 2.4m, 10),
                new LimitOrder("", "", 2.3m, 10),
                new LimitOrder("", "", 2.2m, 10),
                new LimitOrder("", "", 2.0m, 10),
                new LimitOrder("", "", 1.9m, 10),
                new LimitOrder("", "", 1.8m, 10),
                new LimitOrder("", "", 1.7m, 10),
                new LimitOrder("", "", 1.3m, 10),
                new LimitOrder("", "", 1.2m, 10),
                new LimitOrder("", "", 1.1m, 10),
            };

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 3.2m, 1),
                new LimitOrder("", "", 3.1m, 1),
                new LimitOrder("", "", 3m, 1),
                new LimitOrder("", "", 2.5m, 1),
                new LimitOrder("", "", 2.1m, 100),
                new LimitOrder("", "", 1.6m, 5),
                new LimitOrder("", "", 1.5m, 5),
                new LimitOrder("", "", 1.4m, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumePnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(70, volumePnL?.Volume);
            Assert.Equal(40.4m, volumePnL?.PnL);
        }
    }
}
