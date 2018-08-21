using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks;
using Refit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Api
{
    public interface IOrderBooksApi
    {
        [Get("/api/orderBooks")]
        Task<IReadOnlyList<OrderBook>> GetAll();
    }
}
