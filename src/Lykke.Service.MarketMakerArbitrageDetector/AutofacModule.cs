using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Sdk;
using Lykke.Service.Assets.Client;
using Lykke.Service.MarketMakerArbitrageDetector.Managers;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Publishers;
using Lykke.Service.MarketMakerArbitrageDetector.RabbitMQ.Subscribers;
using Lykke.Service.MarketMakerArbitrageDetector.Services.Publishers;
using Lykke.Service.MarketMakerArbitrageDetector.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.MarketMakerArbitrageDetector
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public AutofacModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new AzureRepositories.AutofacModule(
                _settings.Nested(o => o.MarketMakerArbitrageDetectorService.Db.DataConnectionString)));

            builder.RegisterModule(new Services.AutofacModule());

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            RegisterRabbit(builder);

            RegisterClients(builder);
        }

        private void RegisterRabbit(ContainerBuilder builder)
        {
            var exchangesSettings = _settings.CurrentValue.MarketMakerArbitrageDetectorService.RabbitMq;

            builder.RegisterType<LykkeOrderBookSubscriber>()
                .AsSelf()
                .WithParameter(TypedParameter.From(exchangesSettings))
                .SingleInstance();

            builder.RegisterType<MarketMakersPublisher>()
                .As<IMarketMakersPublisher>()
                .WithParameter(TypedParameter.From(exchangesSettings))
                .SingleInstance();
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterInstance(new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new OrderBookProviderClient(_settings.CurrentValue.OrderBooksCacheProviderClient.ServiceUrl))
                .As<IOrderBookProviderClient>()
                .SingleInstance();
        }
    }
}
