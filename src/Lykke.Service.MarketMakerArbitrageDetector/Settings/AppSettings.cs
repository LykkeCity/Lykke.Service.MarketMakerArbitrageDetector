using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.MarketMakerArbitrageDetector.Settings.Clients;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public MarketMakerArbitrageDetectorSettings MarketMakerArbitrageDetectorService { get; set; }

        public AssetsServiceClientSettings AssetsServiceClient { get; set; }

        public OrderBooksCacheProviderClientSettings OrderBooksCacheProviderClient { get; set; }
    }
}
