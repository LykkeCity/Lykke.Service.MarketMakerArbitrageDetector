using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class SynthOrderBookTests
    {
        private readonly AssetPair _btcusd = GetAssetPair("BTC", "USD");
        private readonly AssetPair _btceur = GetAssetPair("BTC", "EUR");
        private readonly AssetPair _eurusd = GetAssetPair("EUR", "USD");
        private readonly AssetPair _gbpusd = GetAssetPair("GBP", "USD");
        private readonly AssetPair _eurchf = GetAssetPair("EUR", "CHF");
        private readonly AssetPair _chfusd = GetAssetPair("CHF", "USD");
        private readonly AssetPair _usdeur = GetAssetPair("USD", "EUR");
        private readonly AssetPair _usdgbp = GetAssetPair("USD", "GBP");
        private readonly AssetPair _eurbtc = GetAssetPair("EUR", "BTC");
        private readonly AssetPair _eurjpy = GetAssetPair("EUR", "JPY");
        private readonly AssetPair _jpyusd = GetAssetPair("JPY", "USD");
        private readonly AssetPair _usdjpy = GetAssetPair("USD", "JPY");
        private readonly AssetPair _jpyeur = GetAssetPair("JPY", "EUR");


        [Fact]
        public void FromOrderBook_0_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9), new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10), new LimitOrder(8999.95m, 7), new LimitOrder(8900.12345677m, 3)
                },
                timestamp);

            var synthOrderBook = SynthOrderBook.FromOrderBook(btcEurOrderBook, _btceur);
            Assert.Equal(exchange, synthOrderBook.Exchange);
            Assert.Equal(_btceur, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8825m, synthOrderBook.BestBid.Price);
            Assert.Equal(9000m, synthOrderBook.Asks.Max(x => x.Price));
            Assert.Equal(8823m, synthOrderBook.Bids.Min(x => x.Price));
            Assert.Equal(8900.12345677m, synthOrderBook.BestAsk.Price);
            Assert.Equal(9, synthOrderBook.BestBid.Volume);
            Assert.Equal(10, synthOrderBook.Asks.Max(x => x.Volume));
            Assert.Equal(5, synthOrderBook.Bids.Min(x => x.Volume));
            Assert.Equal(3, synthOrderBook.BestAsk.Volume);
            Assert.Equal(timestamp, synthOrderBook.Timestamp);
            Assert.Equal(1, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBook_1_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;
            var inverted = _btcusd.Invert();

            var btcUsdOrderBook = new OrderBook(exchange, _btcusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/8825m, 9), new LimitOrder(1/8823m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/9000m, 10), new LimitOrder(1/8999.95m, 7), new LimitOrder(1/8900.12345677m, 3)
                },
                timestamp);

            var synthOrderBook = SynthOrderBook.FromOrderBook(btcUsdOrderBook, inverted);
            Assert.Equal(exchange, synthOrderBook.Exchange);
            Assert.Equal(inverted, synthOrderBook.AssetPair);
            Assert.Equal("BTCUSD", synthOrderBook.ConversionPath);
            Assert.Equal(3, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(9000m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8825m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8900.12345677m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8823m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00111111m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00101983m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00033707m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00056670m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp, synthOrderBook.Timestamp);
            Assert.Equal(1, synthOrderBook.OriginalOrderBooks.Count);
        }


        [Fact]
        public void From2OrderBooks_0_0_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9),
                    new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10),
                    new LimitOrder(8999.95m, 7),
                    new LimitOrder(8900.12345677m, 3)
                },
                timestamp1);

            var eurUsdOrderBook = new OrderBook(exchange, _eurusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1.11m, 9),
                    new LimitOrder(1.10m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1.12m, 10),
                    new LimitOrder(1.13m, 7),
                    new LimitOrder(1.14m, 3)
                },
                timestamp2);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, _btcusd);
            Assert.Equal("FakeExchange - FakeExchange", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & EURUSD", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(9795.75m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(9968.1382715824m, synthOrderBook.BestAsk.Price, 8);
            // TODO: Prices must be tested in all From* methods (i.e. GetOrderedLimitOrders methods)
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooks_0_1_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(8825, 9),
                    new LimitOrder(8823, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(9000, 10),
                    new LimitOrder(8999.95m, 7),
                    new LimitOrder(8900.12345677m, 3)
                },
                timestamp1);

            var eurUsdOrderBook = new OrderBook(exchange, _usdeur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/1.11m, 9),
                    new LimitOrder(1/1.10m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/1.12m, 10),
                    new LimitOrder(1/1.13m, 7),
                    new LimitOrder(1/1.14m, 3)
                },
                timestamp2);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, _btcusd);
            Assert.Equal("FakeExchange - FakeExchange", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & USDEUR", synthOrderBook.ConversionPath);
            Assert.Equal(3, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(10060.5m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(9790.135802447m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00029820m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00051072m, synthOrderBook.BestAsk.Volume, 8);
            // TODO: Prices must be tested (i.e. GetOrderedLimitOrders methods)
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooks_1_0_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/8825m, 9),
                    new LimitOrder(1/8823m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/9000m, 10),
                    new LimitOrder(1/8999.95m, 7),
                    new LimitOrder(1/8900.12345677m, 3)
                },
                timestamp1);

            var eurUsdOrderBook = new OrderBook(exchange, _eurusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1.11m, 9),
                    new LimitOrder(1.10m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1.12m, 10),
                    new LimitOrder(1.13m, 7),
                    new LimitOrder(1.14m, 3)
                },
                timestamp2);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, _btcusd);
            Assert.Equal("FakeExchange - FakeExchange", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & EURUSD", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(9990m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(9881.76m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.001m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00056670m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooks_1_1_Test()
        {
            const string exchange = "FakeExchange";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/8825m, 9),
                    new LimitOrder(1/8823m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/9000m, 10),
                    new LimitOrder(1/8999.95m, 7),
                    new LimitOrder(1/8900.12345677m, 3)
                },
                timestamp1);

            var eurUsdOrderBook = new OrderBook(exchange, _usdeur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/1.11m, 9),
                    new LimitOrder(1/1.10m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/1.12m, 10),
                    new LimitOrder(1/1.13m, 7),
                    new LimitOrder(1/1.14m, 3)
                },
                timestamp2);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, _btcusd);
            Assert.Equal("FakeExchange - FakeExchange", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & USDEUR", synthOrderBook.ConversionPath);
            Assert.Equal(3, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(10260m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(9705.30m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00029240m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00051518m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }



        [Fact]
        public void From3OrderBooks_0_0_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(7310m, 9), new LimitOrder(7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(7320m, 10), new LimitOrder(7330m, 7), new LimitOrder(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _eurjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(131m, 9), new LimitOrder(130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(132m, 11), new LimitOrder(133m, 7), new LimitOrder(134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _jpyusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(0.009132m, 9), new LimitOrder(0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(0.009133m, 12), new LimitOrder(0.009134m, 7), new LimitOrder(0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & EURJPY & JPYUSD", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8744.894520m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8824.669920m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00000940m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00001242m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_0_0_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(7310m, 9), new LimitOrder(7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(7320m, 10), new LimitOrder(7330m, 7), new LimitOrder(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _eurjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(131m, 9), new LimitOrder(130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(132m, 11), new LimitOrder(133m, 7), new LimitOrder(134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _usdjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/0.009132m, 9), new LimitOrder(1/0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/0.009133m, 12), new LimitOrder(1/0.009134m, 7), new LimitOrder(1/0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & EURJPY & USDJPY", synthOrderBook.ConversionPath);
            Assert.Equal(4, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(8747.767350m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8822.737440m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00034294m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00056672m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_0_1_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(7310m, 9), new LimitOrder(7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(7320m, 10), new LimitOrder(7330m, 7), new LimitOrder(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _eurjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(131m, 9), new LimitOrder(130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(132m, 11), new LimitOrder(133m, 7), new LimitOrder(134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _jpyusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(0.009132m, 9), new LimitOrder(0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(0.009133m, 12), new LimitOrder(0.009134m, 7), new LimitOrder(0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & EURJPY & JPYUSD", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8744.894520m, synthOrderBook.BestBid.Price);
            Assert.Equal(8824.669920m, synthOrderBook.BestAsk.Price);
            Assert.Equal(0.00000940m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00001242m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_1_0_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/7310m, 9), new LimitOrder(1/7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/7320m, 10), new LimitOrder(1/7330m, 7), new LimitOrder(1/7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _eurjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(131m, 9), new LimitOrder(130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(132m, 11), new LimitOrder(133m, 7), new LimitOrder(134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _jpyusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(0.009132m, 9), new LimitOrder(0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(0.009133m, 12), new LimitOrder(0.009134m, 7), new LimitOrder(0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & EURJPY & JPYUSD", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8780.78328m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8800.5588m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00000936m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00001245m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_0_1_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _btceur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(7310m, 9), new LimitOrder(7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(7320m, 10), new LimitOrder(7330m, 7), new LimitOrder(7340m, 3)
                },
                timestamp1);

            var jpyEurOrderBook = new OrderBook(exchange2, _jpyeur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/132m, 11), new LimitOrder(1/133m, 7), new LimitOrder(1/134m, 3)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/131m, 9), new LimitOrder(1/130m, 5)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _usdjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/0.009132m, 9), new LimitOrder(1/0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/0.009133m, 12), new LimitOrder(1/0.009134m, 7), new LimitOrder(1/0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, jpyEurOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("BTCEUR & JPYEUR & USDJPY", synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8747.76735m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8822.73744m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00000940m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00001138m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_1_0_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/7310m, 9), new LimitOrder(1/7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/7320m, 10), new LimitOrder(1/7330m, 7), new LimitOrder(1/7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _eurjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(131m, 9), new LimitOrder(130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(132m, 11), new LimitOrder(133m, 7), new LimitOrder(134m, 3)
                },
                timestamp2);
            
            var jpyUsdOrderBook = new OrderBook(exchange3, _usdjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/0.009132m, 9), new LimitOrder(1/0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/0.009133m, 12), new LimitOrder(1/0.009134m, 7), new LimitOrder(1/0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & EURJPY & USDJPY", synthOrderBook.ConversionPath);
            Assert.Equal(6, synthOrderBook.Bids.Count());
            Assert.Equal(3, synthOrderBook.Asks.Count());
            Assert.Equal(8783.6679m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8798.6316m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00034154m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00056827m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_1_1_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/7310m, 9), new LimitOrder(1/7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/7320m, 10), new LimitOrder(1/7330m, 7), new LimitOrder(1/7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _jpyeur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/131m, 9), new LimitOrder(1/130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/132m, 11), new LimitOrder(1/133m, 7), new LimitOrder(1/134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _jpyusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(0.009132m, 9), new LimitOrder(0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(0.009133m, 12), new LimitOrder(0.009134m, 7), new LimitOrder(0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & JPYEUR & JPYUSD", synthOrderBook.ConversionPath);
            Assert.Equal(4, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(8981.86992m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8667.217m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00000305m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00000527m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooks_1_1_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var btcEurOrderBook = new OrderBook(exchange1, _eurbtc,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/7310m, 9), new LimitOrder(1/7300m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/7320m, 10), new LimitOrder(1/7330m, 7), new LimitOrder(1/7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, _jpyeur,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/131m, 9), new LimitOrder(1/130m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/132m, 11), new LimitOrder(1/133m, 7), new LimitOrder(1/134m, 3)
                },
                timestamp2);

            var jpyUsdOrderBook = new OrderBook(exchange3, _usdjpy,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1/0.009132m, 9), new LimitOrder(1/0.009131m, 5)
                },
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1/0.009133m, 12), new LimitOrder(1/0.009134m, 7), new LimitOrder(1/0.009135m, 3)
                },
                timestamp3);

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, _btcusd);
            Assert.Equal("TEST1 - TEST2 - TEST3", synthOrderBook.Exchange);
            Assert.Equal(_btcusd, synthOrderBook.AssetPair);
            Assert.Equal("EURBTC & JPYEUR & USDJPY", synthOrderBook.ConversionPath);
            Assert.Equal(3, synthOrderBook.Bids.Count());
            Assert.Equal(2, synthOrderBook.Asks.Count());
            Assert.Equal(8984.8206m, synthOrderBook.BestBid.Price, 8);
            Assert.Equal(8665.319m, synthOrderBook.BestAsk.Price, 8);
            Assert.Equal(0.00000305m, synthOrderBook.BestBid.Volume, 8);
            Assert.Equal(0.00000527m, synthOrderBook.BestAsk.Volume, 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }




        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_Test()
        {
            var orderBook = new OrderBook("FE", _btcusd, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);
            var result = SynthOrderBook.PrepareForEnumeration(new List<OrderBook> { orderBook }, _btcusd);

            Assert.Single(result);
            Assert.True(result.Single().Key.Equals(_btcusd));
            Assert.True(result.Single().Value.AssetPair.EqualOrInverted(_btcusd));
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_Test()
        {
            var usdbtc = _btcusd.Invert();
            var orderBook = new OrderBook("FE", usdbtc, new List<LimitOrder>(), new List<LimitOrder>(), DateTime.UtcNow);

            var result = SynthOrderBook.PrepareForEnumeration(new List<OrderBook> { orderBook }, _btcusd);

            Assert.Single(result);
            Assert.True(result.Single().Key.Equals(_btcusd));
            Assert.True(result.Single().Value.AssetPair.EqualOrInverted(_btcusd));
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
            var orderBooks = GetOrderBooks(_btceur, _eurusd.Invert());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurusd.Invert());

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
            var orderBooks = GetOrderBooks(_btceur, _eurchf, _chfusd.Invert());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf.Invert(), _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurchf, _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_0_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur, _eurchf.Invert(), _chfusd.Invert());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_0_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurchf.Invert(), _chfusd);

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_0_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurchf, _chfusd.Invert());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_PrepareForEnumeration_1_1_1_Test()
        {
            var orderBooks = GetOrderBooks(_btceur.Invert(), _eurchf.Invert(), _chfusd.Invert());

            var result = SynthOrderBook.PrepareForEnumeration(orderBooks, _btcusd);

            AssertChained3(result);
        }


        [Fact]
        public void SynthOrderBook_GetBids_Streight_Test()
        {
            var gbpusdOb = new OrderBook("FE", _gbpusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1.28167m, 2909.98m),
                    new LimitOrder(1.29906m, 50000m)
                },
                new List<LimitOrder>(), // asks
                DateTime.Now);

            var bids = SynthOrderBook.GetBids(gbpusdOb, _gbpusd).ToList();
            var orderedBids = bids.OrderByDescending(x => x.Price).ToList();

            Assert.Equal(2, bids.Count);
            Assert.Equal(bids[0].Price, orderedBids[0].Price);
            Assert.Equal(bids[1].Price, orderedBids[1].Price);
        }

        [Fact]
        public void SynthOrderBook_GetBids_Inverted_Test()
        {
            var gbpusdOb = new OrderBook("FE", _gbpusd,
                new List<LimitOrder>(), // bids
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1.29906m, 50000m),
                    new LimitOrder(1.28167m, 2909.98m)
                },
                DateTime.Now);

            var bids = SynthOrderBook.GetBids(gbpusdOb, _usdgbp).ToList();
            var orderedBids = bids.OrderByDescending(x => x.Price).ToList();

            Assert.Equal(2, bids.Count);
            Assert.Equal(bids[0].Price, orderedBids[0].Price);
            Assert.Equal(bids[1].Price, orderedBids[1].Price);
        }

        [Fact]
        public void SynthOrderBook_GetAsks_Streight_Test()
        {
            var gbpusdOb = new OrderBook("FE", _gbpusd,
                new List<LimitOrder>(), // bids
                new List<LimitOrder> // asks
                {
                    new LimitOrder(1.29906m, 50000m),
                    new LimitOrder(1.28167m, 2909.98m)
                },
                DateTime.Now);

            var bids = SynthOrderBook.GetAsks(gbpusdOb, _gbpusd).ToList();
            var orderedBids = bids.OrderBy(x => x.Price).ToList();

            Assert.Equal(2, bids.Count);
            Assert.Equal(bids[0].Price, orderedBids[0].Price);
            Assert.Equal(bids[1].Price, orderedBids[1].Price);
        }

        [Fact]
        public void SynthOrderBook_GetAsks_Inverted_Test()
        {
            var gbpusdOb = new OrderBook("FE", _gbpusd,
                new List<LimitOrder> // bids
                {
                    new LimitOrder(1.28167m, 2909.98m),
                    new LimitOrder(1.29906m, 50000m)
                },
                new List<LimitOrder>(), // asks
                DateTime.Now);

            var bids = SynthOrderBook.GetAsks(gbpusdOb, _usdgbp).ToList();
            var orderedBids = bids.OrderBy(x => x.Price).ToList();

            Assert.Equal(2, bids.Count);
            Assert.Equal(bids[0].Price, orderedBids[0].Price);
            Assert.Equal(bids[1].Price, orderedBids[1].Price);
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
            Assert.True(result.ElementAt(0).Value.AssetPair.EqualOrInverted(_btceur));

            Assert.Equal("EUR", result.ElementAt(1).Key.Base.Id);
            Assert.Equal("USD", result.ElementAt(1).Key.Quote.Id);
            Assert.True(result.ElementAt(1).Value.AssetPair.EqualOrInverted(_eurusd));
        }

        private void AssertChained3(IDictionary<AssetPair, OrderBook> result)
        {
            Assert.Equal(3, result.Count);

            Assert.Equal("BTC", result.ElementAt(0).Key.Base.Id);
            Assert.Equal("EUR", result.ElementAt(0).Key.Quote.Id);
            Assert.True(result.ElementAt(0).Value.AssetPair.EqualOrInverted(_btceur));

            Assert.Equal("EUR", result.ElementAt(1).Key.Base.Id);
            Assert.Equal("CHF", result.ElementAt(1).Key.Quote.Id);
            Assert.True(result.ElementAt(1).Value.AssetPair.EqualOrInverted(_eurchf));

            Assert.Equal("CHF", result.ElementAt(2).Key.Base.Id);
            Assert.Equal("USD", result.ElementAt(2).Key.Quote.Id);
            Assert.True(result.ElementAt(2).Value.AssetPair.EqualOrInverted(_chfusd));
        }


        private static AssetPair GetAssetPair(string @base, string quote)
        {
            return new AssetPair($"{@base}{quote}", $"{@base}{quote}", new Asset(@base, @base), new Asset(quote, quote), 8, 8);
        }
    }
}
