using Autofac;
using JetBrains.Annotations;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Handlers;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Lykke.Service.MarketMakerArbitrageDetector.Services.OrderBooks;

namespace Lykke.Service.MarketMakerArbitrageDetector.Services
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOrderBooks(builder);
        }

        private void RegisterOrderBooks(ContainerBuilder builder)
        {
            builder.RegisterType<LykkeOrderBookService>()
                .As<ILykkeOrderBookService>()
                .As<ILykkeOrderBookHandler>()
                .SingleInstance();
        }
    }
}
