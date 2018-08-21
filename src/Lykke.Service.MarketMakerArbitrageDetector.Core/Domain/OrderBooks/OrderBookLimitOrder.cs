namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class OrderBookLimitOrder
    {
        public string OrderId { get; set; }

        public string ClientId { get; set; }

        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public OrderBookLimitOrder()
        {
        }

        public OrderBookLimitOrder(string orderId, string clientId, decimal volume, decimal price)
        {
            OrderId = orderId;
            ClientId = clientId;
            Volume = volume;
            Price = price;
        }
    }
}
