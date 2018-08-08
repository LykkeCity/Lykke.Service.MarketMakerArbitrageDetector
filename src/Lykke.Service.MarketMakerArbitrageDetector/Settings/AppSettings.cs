using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public MarketMakerArbitrageDetectorSettings MarketMakerArbitrageDetectorService { get; set; }
    }
}
