using JetBrains.Annotations;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    [PublicAPI]
    public interface IMarketMakerArbitrageDetectorClient
    {
        IOrderBooksApi OrderBooks { get; }

        IArbitragesApi Arbitrages { get; }
    }
}
