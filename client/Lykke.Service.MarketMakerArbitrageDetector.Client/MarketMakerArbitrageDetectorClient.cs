using Lykke.HttpClientGenerator;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    public class MarketMakerArbitrageDetectorClient : IMarketMakerArbitrageDetectorClient
    {
        public IOrderBooksApi OrderBooks { get; }
        public IArbitragesApi Arbitrages { get; }

        public MarketMakerArbitrageDetectorClient(IHttpClientGenerator httpClientGenerator)
        {
            OrderBooks = httpClientGenerator.Generate<IOrderBooksApi>();
            Arbitrages = httpClientGenerator.Generate<IArbitragesApi>();
        }
    }
}
