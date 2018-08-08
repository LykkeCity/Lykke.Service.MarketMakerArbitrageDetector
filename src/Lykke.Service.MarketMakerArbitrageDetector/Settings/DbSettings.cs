using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
