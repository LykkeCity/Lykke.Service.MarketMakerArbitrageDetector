namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class LimitOrder
    {
        public string OrderId { get; }

        public string ClientId { get; }

        public decimal Volume { get; }

        public decimal Price { get; }

        public LimitOrder(string orderId, string clientId, decimal volume, decimal price)
        {
            OrderId = orderId;
            ClientId = clientId;
            Volume = volume;
            Price = price;
        }
    }
}
