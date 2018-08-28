namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class LimitOrder
    {
        public string OrderId { get; }

        public string ClientId { get; }

        public decimal Price { get; }

        public decimal Volume { get; set; }


        public LimitOrder(string orderId, string clientId, decimal price, decimal volume)
        {
            OrderId = orderId;
            ClientId = clientId;
            Price = price;
            Volume = volume;
        }

        public LimitOrder(decimal price, decimal volume)
        {
            Price = price;
            Volume = volume;
        }

        public LimitOrder Reciprocal()
        {
            return new LimitOrder(OrderId, ClientId, 1 / Price, Volume * Price);
        }

        public LimitOrder Clone()
        {
            return new LimitOrder(OrderId, ClientId, Price, Volume);
        }

        public LimitOrder CloneWithoutIds()
        {
            return new LimitOrder(Price, Volume);
        }

        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, ClientId: {ClientId}";
        }
    }
}
