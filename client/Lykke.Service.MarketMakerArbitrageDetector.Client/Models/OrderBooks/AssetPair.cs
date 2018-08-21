namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks
{
    public class AssetPair
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Asset Base { get; set; }

        public Asset Quote { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
