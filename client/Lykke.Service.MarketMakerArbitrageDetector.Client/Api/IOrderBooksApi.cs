using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IOrderBooksApi
    {
        [Get("/api/orderBooks")]
        Task<IReadOnlyCollection<OrderBook>> GetAllAsync();
    }
}
