using JetBrains.Annotations;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    /// <summary>
    /// MarketMakerArbitrageDetector client interface.
    /// </summary>
    [PublicAPI]
    public interface IMarketMakerArbitrageDetectorClient
    {
        /// <summary>Application Api interface</summary>
        IMarketMakerArbitrageDetectorApi Api { get; }
    }
}
