using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string DataConnectionString { get; set; }

        [AzureTableCheck]
        public string LogsConnectionString { get; set; }
    }
}
