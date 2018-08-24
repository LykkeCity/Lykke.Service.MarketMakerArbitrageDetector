using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface IOrderBooksService
    {
        IReadOnlyCollection<OrderBook> GetAll();

        IReadOnlyCollection<OrderBookRow> GetAllRows();

        OrderBook Get(string assetPairId);

        decimal? ConvertToUsd(string sourceAssetId, decimal value, int accuracy = 0);
    }
}
