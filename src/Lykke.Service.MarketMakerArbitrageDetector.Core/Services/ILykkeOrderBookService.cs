using System.Collections.Generic;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.OrderBooks;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Services
{
    public interface ILykkeOrderBookService
    {
        IReadOnlyList<OrderBook> GetAll();
    }
}
