using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;

namespace Lykke.Service.MarketMakerArbitrageDetector
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Core.Domain.OrderBook, OrderBook>();
            CreateMap<Core.Domain.Arbitrage, Arbitrage>();
        }
    }
}
