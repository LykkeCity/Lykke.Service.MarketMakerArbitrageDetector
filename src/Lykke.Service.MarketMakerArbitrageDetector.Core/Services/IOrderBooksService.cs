using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface IOrderBooksService
    {
        IReadOnlyList<OrderBook> GetAll();

        IReadOnlyList<OrderBookRow> GetAllRows(bool wantedOnly = true);

        IReadOnlyList<OrderBook> GetFilteredByWallets();

        OrderBook Get(string assetPairId);

        decimal? ConvertToUsd(string sourceAssetId, decimal value, int accuracy = 0);
    }
}
