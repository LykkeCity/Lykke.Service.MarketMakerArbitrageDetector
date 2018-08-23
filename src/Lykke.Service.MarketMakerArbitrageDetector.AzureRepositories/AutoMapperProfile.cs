using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories.Entities;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;

namespace Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories
{
    [UsedImplicitly]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SettingsEntity, Settings>(MemberList.Destination);
            CreateMap<Settings, SettingsEntity>(MemberList.Source);
        }
    }
}
