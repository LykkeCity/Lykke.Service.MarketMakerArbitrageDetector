using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; set; }

        public string AssetPair { get; set; }

        public IReadOnlyList<OrderBookLimitOrder> SellLimitOrders { get; set; }

        public IReadOnlyList<OrderBookLimitOrder> BuyLimitOrders { get; set; }

        public DateTime Timestamp { get; set; }

        public OrderBook(string exchange, string assetPair, IReadOnlyList<OrderBookLimitOrder> buyLimitOrders, IReadOnlyList<OrderBookLimitOrder> sellLimitOrders, DateTime timestamp)
        {
            Exchange = !string.IsNullOrWhiteSpace(exchange) ? exchange : throw new ArgumentException($"Argument '{nameof(exchange)}' is null or empty.");
            AssetPair = !string.IsNullOrWhiteSpace(assetPair) ? assetPair : throw new ArgumentException($"Argument '{nameof(assetPair)}' is null or empty.");
            BuyLimitOrders = buyLimitOrders ?? throw new ArgumentNullException($"Argument '{nameof(buyLimitOrders)}' is null.");
            SellLimitOrders = sellLimitOrders ?? throw new ArgumentNullException($"Argument '{nameof(sellLimitOrders)}' is null.");
            Timestamp = timestamp;
        }
    }
}
