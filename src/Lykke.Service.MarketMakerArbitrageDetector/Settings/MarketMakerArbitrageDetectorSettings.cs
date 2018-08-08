using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MarketMakerArbitrageDetectorSettings
    {
        public DbSettings Db { get; set; }
    }
}
