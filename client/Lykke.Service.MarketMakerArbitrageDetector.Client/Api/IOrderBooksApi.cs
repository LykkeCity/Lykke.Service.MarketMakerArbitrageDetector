using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IOrderBooksApi
    {
        /// <summary>
        /// Returns all summary information about all order books.
        /// </summary>
        [Get("/api/orderBooks")]
        Task<IReadOnlyCollection<OrderBookRow>> GetAllRowsAsync(bool wantedOnly = true);

        /// <summary>
        /// Returns an order book by asset pair id.
        /// </summary>
        [Get("/api/orderBooks/{assetPairId}")]
        Task<OrderBook> GetAsync(string assetPairId);
    }
}
