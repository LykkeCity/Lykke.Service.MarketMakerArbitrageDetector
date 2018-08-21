using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.Settings;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Repositories;
using Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories.Entities;

namespace Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        public static string GetPartitionKey() => "Settings";
        private static string GetRowKey() => "Settings";

        private readonly INoSQLTableStorage<SettingsEntity> _storage;

        public SettingsRepository(INoSQLTableStorage<SettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Settings> GetAsync()
        {
            var entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return Mapper.Map<Settings>(entity);
        }

        public async Task InsertOrReplaceAsync(Settings settings)
        {
            var entity = new SettingsEntity(GetPartitionKey(), GetRowKey());

            Mapper.Map(settings, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }
    }
}
