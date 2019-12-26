using JetBrains.Annotations;

namespace Lykke.Service.MarketMakerArbitrageDetector.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MarketMakerArbitrageDetectorSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public MainRabbitMqSettings MainRabbitMq { get; set; }
    }
}
