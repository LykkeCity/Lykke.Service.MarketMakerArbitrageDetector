namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks
{
    public class Asset
    {
        public string Id { get; }

        public string Name { get; }

        public Asset(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
