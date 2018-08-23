using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Handlers
{
    public interface ILykkeOrderBookHandler
    {
        Task HandleAsync(OrderBook orderBook);
    }
}
