
using System.Threading.Tasks;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Repositories;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private Core.Domain.Settings.Settings _settings;
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Core.Domain.Settings.Settings> GetAsync()
        {
            if (_settings == null)
                _settings = await _settingsRepository.GetAsync();

            return _settings;
        }

        public async Task SetAsync(Core.Domain.Settings.Settings settings)
        {
            _settings = settings;

            await _settingsRepository.InsertOrReplaceAsync(_settings);
        }
    }
}
