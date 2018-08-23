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
            Volume = volume;
            Price = price;
        }

        public LimitOrder Reciprocal()
        {
            return new LimitOrder(OrderId, ClientId, 1 / Price, Volume * Price);
        }

        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, ClientId: {ClientId}";
        }
    }
}
