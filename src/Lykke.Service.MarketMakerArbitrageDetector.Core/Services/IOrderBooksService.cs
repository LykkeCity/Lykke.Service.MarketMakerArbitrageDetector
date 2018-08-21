using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface IOrderBooksService
    {
        Task<IReadOnlyList<OrderBook>> GetAllAsync();
    }
}
