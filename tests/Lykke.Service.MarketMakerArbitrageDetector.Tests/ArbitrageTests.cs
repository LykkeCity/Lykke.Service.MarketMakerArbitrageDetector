using System;
using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class ArbitrageTests
    {
        private readonly AssetPair _btcusd = GetAssetPair("BTC", "USD");
        private readonly AssetPair _eurgbp = GetAssetPair("EUR", "GBP");
        private readonly AssetPair _eurusd = GetAssetPair("EUR", "USD");
        private readonly AssetPair _gbpusd = GetAssetPair("GBP", "USD");


        [Fact]
        public void ArbitrageVolume_NoArbitrage_EmptyOrderBooks_Test()
        {
            const string exchangeName = "FE";
            var timestamp = DateTime.UtcNow;

            var orderBook1 = new OrderBook(exchangeName, _btcusd, new List<LimitOrder>(), new List<LimitOrder>(), timestamp);
            var orderBook2 = new OrderBook(exchangeName, _btcusd, new List<LimitOrder>(), new List<LimitOrder>(), timestamp);

            var volume = Arbitrage.GetArbitrageVolumeAndPnL(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_NoArbitrage_TheSameOrderBook_Test()
        {
            const string exchangeName = "FE";
            var timestamp = DateTime.UtcNow;

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 8825, 9),
                new LimitOrder("", "", 8823, 5)
            };
            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 10),
                new LimitOrder("", "", 8999.95m, 7),
                new LimitOrder("", "", 8900.12345677m, 3)
            };

            var orderBook1 = new OrderBook(exchangeName, _btcusd, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, _btcusd, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolumeAndPnL(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolumePnL_Simple1_Test()
        {
            const string exchangeName = "FE";
            var timestamp = DateTime.UtcNow;

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 9), // <-
                new LimitOrder("", "", 8900, 5)
            };
            var asks = new List<LimitOrder>
            {
                new LimitOrder("", "", 9000, 10),
                new LimitOrder("", "", 8999.95m, 7), // <-
                new LimitOrder("", "", 8900.12345677m, 3) // <-
            };

            var bidsOrderBook = new OrderBook(exchangeName, _btcusd, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, _btcusd, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(9, volumePnL?.Volume);
            Assert.Equal(299.92962969m, volumePnL?.PnL);
        }

        [Fact]
        public void ArbitrageVolumePnL_Simple2_Test()
        {
            var eurgbpOB = new OrderBook("FE", _eurgbp,
                new List<LimitOrder> (), // bids
                new List<LimitOrder> // asks
                {
                    new LimitOrder(0.90477m, 757921.29m)
                },
                DateTime.Now);

            var eurusdOB = new OrderBook("FE", _eurusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1.16211m, 1923.11m),
                    new LimitOrder(0.58117m, 100)
                },
                new List<LimitOrder>(), // asks
                DateTime.Now);

            var gbpusdOB = new OrderBook("FE", _gbpusd,
                new List<LimitOrder>(), // bids
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1.28167m, 2909.98m), // 0.78023, 3729.63406 (reciprocal)
                    new LimitOrder(1.29906m, 50000m)    // 0.76978, 64953.0 (reciprocal)
                },
                DateTime.Now);

            var eurgbpSynth = SynthOrderBook.FromOrderBooks(eurusdOB, gbpusdOB, _eurgbp);

            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(eurgbpSynth.Bids, eurgbpOB.Asks);
            Assert.NotNull(volumePnL?.Volume);
            Assert.NotNull(volumePnL?.PnL);
        }

        [Fact]
        public void ArbitrageVolumePnL_Complex1_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=0

            const string exchangeName = "FE";
            var timestamp = DateTime.UtcNow;

            var bids = new List<LimitOrder>
            {
                new LimitOrder("", "", 900, 5),   // <-
                new LimitOrder("", "", 750, 100), // <-
                new LimitOrder("", "", 550, 1)    // <-
            };
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

            var bidsOrderBook = new OrderBook(exchangeName, _btcusd, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, _btcusd, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(41, volumePnL?.Volume);
            Assert.Equal(6450, volumePnL?.PnL);
        }

        [Fact]
        public void ArbitrageVolumePnL_Complex2_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=2011486790
            const string exchangeName = "FE";
            var timestamp = DateTime.UtcNow;

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

            var bidsOrderBook = new OrderBook(exchangeName, _btcusd, bids, new List<LimitOrder>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, _btcusd, new List<LimitOrder>(), asks, timestamp);

            var volumePnL = Arbitrage.GetArbitrageVolumeAndPnL(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(70, volumePnL?.Volume);
            Assert.Equal(40.4m, volumePnL?.PnL);
        }


        private static AssetPair GetAssetPair(string @base, string quote)
        {
            return new AssetPair($"{@base}{quote}", $"{@base}{quote}", new Asset(@base, @base), new Asset(quote, quote), 8, 8);
        }
    }
}
