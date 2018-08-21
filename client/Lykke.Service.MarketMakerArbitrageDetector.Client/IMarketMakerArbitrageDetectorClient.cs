using JetBrains.Annotations;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    /// <summary>
    /// MarketMakerArbitrageDetector client interface.
    /// </summary>
    [PublicAPI]
    public interface IMarketMakerArbitrageDetectorClient
    {
        /// <summary>Order books API interface</summary>
        IOrderBooksApi OrderBooksApi { get; }
    }
}
