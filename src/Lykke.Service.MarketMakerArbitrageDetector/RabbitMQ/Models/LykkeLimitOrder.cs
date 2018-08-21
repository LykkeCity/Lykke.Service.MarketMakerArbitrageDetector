namespace Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Models
{
    public class LykkeLimitOrder
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
    }
}
