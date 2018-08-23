using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Repositories;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services
{
    public class SettingsService : ISettingsService
    {
        private object _sync = new object();
        private Settings _settings;
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public Settings Get()
        {
            lock (_sync)
            {
                if (_settings == null)
                    _settings = _settingsRepository.GetAsync().GetAwaiter().GetResult();

                if (_settings == null)
                    _settingsRepository.InsertOrReplaceAsync(new Settings()).GetAwaiter().GetResult();

                return _settings;
            }
        }

        public void Set(Settings settings)
        {
            lock (_sync)
            {
                _settings = settings;
                _settingsRepository.InsertOrReplaceAsync(settings).GetAwaiter().GetResult();
            }
        }
    }
}
