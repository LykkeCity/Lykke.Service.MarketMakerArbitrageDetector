namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks
{
    public class LimitOrder
    {
        public string OrderId { get; set; }

        public string ClientId { get; set; }

        public decimal Volume { get; set; }

        public decimal Price { get; set; }
    }
}
