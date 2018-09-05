using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Repositories
{
    public interface ISettingsRepository
    {
        Task<Settings> GetAsync();

        Task InsertOrReplaceAsync(Settings settings);
    }
}
