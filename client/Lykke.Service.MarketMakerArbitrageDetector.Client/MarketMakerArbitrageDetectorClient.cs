using Lykke.HttpClientGenerator;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    /// <summary>
    /// MarketMakerArbitrageDetector API aggregating interface.
    /// </summary>
    public class MarketMakerArbitrageDetectorClient : IMarketMakerArbitrageDetectorClient
    {
        /// <inheritdoc />
        public IOrderBooksApi OrderBooksApi { get; }

        /// <summary>C-tor</summary>
        public MarketMakerArbitrageDetectorClient(IHttpClientGenerator httpClientGenerator)
        {
            OrderBooksApi = httpClientGenerator.Generate<IOrderBooksApi>();
        }
    }
}
