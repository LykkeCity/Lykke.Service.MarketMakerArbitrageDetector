using System;
using System.Collections.Generic;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class OrderBook
    {
        public string Exchange { get; }

        public AssetPair AssetPair { get; private set; }

        public IReadOnlyList<LimitOrder> SellLimitOrders { get; }

        public IReadOnlyList<LimitOrder> BuyLimitOrders { get; }

        public DateTime Timestamp { get; }

        public OrderBook(string exchange, AssetPair assetPair, IReadOnlyList<LimitOrder> buyLimitOrders, IReadOnlyList<LimitOrder> sellLimitOrders, DateTime timestamp)
        {
            Exchange = !string.IsNullOrWhiteSpace(exchange) ? exchange : throw new ArgumentException($"Argument '{nameof(exchange)}' is null or empty.");
            AssetPair = assetPair != null ? assetPair : throw new ArgumentNullException($"Argument '{nameof(assetPair)}' is null.");
            BuyLimitOrders = buyLimitOrders;
            SellLimitOrders = sellLimitOrders;
            Timestamp = timestamp;
        }

        public void SetAssetPair(AssetPair assetPair)
        {
            AssetPair = assetPair;
        }
    }
}
