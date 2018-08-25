namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class AssetPair
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Asset Base { get; }

        public Asset Quote { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
