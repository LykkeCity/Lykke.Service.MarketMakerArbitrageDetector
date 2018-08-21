namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class AssetPair
    {
        public string Id { get; }

        public string Name { get; }

        public Asset Base { get; }

        public Asset Quote { get; }

        public AssetPair(string id)
        {
            Id = id;
        }

        public AssetPair(string id, string name, Asset @base, Asset quote)
        {
            Id = id;
            Name = name;
            Base = @base;
            Quote = quote;
        }
    }
}
