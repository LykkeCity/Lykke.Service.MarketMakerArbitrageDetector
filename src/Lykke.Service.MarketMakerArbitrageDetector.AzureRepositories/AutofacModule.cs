using Autofac;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories.Entities;
using Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories.Repositories;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _connectionString;

        public AutofacModule(IReloadingManager<string> connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(container => new SettingsRepository(
                        AzureTableStorage<SettingsEntity>.Create(_connectionString, nameof(Settings), container.Resolve<ILogFactory>())))
                .As<ISettingsRepository>()
                .SingleInstance();
        }
    }
}
