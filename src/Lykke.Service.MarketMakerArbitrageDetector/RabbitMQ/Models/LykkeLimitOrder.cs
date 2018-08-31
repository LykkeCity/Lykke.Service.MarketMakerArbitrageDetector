using Newtonsoft.Json;

namespace Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Models
{
    public class LykkeLimitOrder
    {
        public string Id { get; set; }

        [JsonProperty("ClientId")]
        public string WalletId { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
    }
}
