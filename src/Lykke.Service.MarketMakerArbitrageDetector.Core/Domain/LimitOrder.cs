using System;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class LimitOrder
    {
        public string OrderId { get; }

        public string ClientId { get; }

        public decimal Price { get; }

        public decimal Volume { get; }


        public LimitOrder(string orderId, string clientId, decimal price, decimal volume)
        {
            OrderId = orderId;
            ClientId = clientId;
            Price = price;
            Volume = volume;
        }

        public LimitOrder Reciprocal()
        {
            return new LimitOrder(OrderId, ClientId, Math.Round(1 / Price, 6), Math.Round(Volume * Price, 6));
        }

        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, ClientId: {ClientId}";
        }
    }
}
