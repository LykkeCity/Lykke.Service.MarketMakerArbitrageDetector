using Lykke.HttpClientGenerator;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    /// <summary>
    /// MarketMakerArbitrageDetector API aggregating interface.
    /// </summary>
    public class MarketMakerArbitrageDetectorClient : IMarketMakerArbitrageDetectorClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to MarketMakerArbitrageDetector Api.</summary>
        public IMarketMakerArbitrageDetectorApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public MarketMakerArbitrageDetectorClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IMarketMakerArbitrageDetectorApi>();
        }
    }
}
