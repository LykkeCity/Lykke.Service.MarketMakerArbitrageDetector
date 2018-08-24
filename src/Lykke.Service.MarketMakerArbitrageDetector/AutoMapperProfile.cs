using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;

namespace Lykke.Service.MarketMakerArbitrageDetector
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client.Models.Settings, Core.Domain.Settings>();
            CreateMap<Core.Domain.Settings, Client.Models.Settings>();
            CreateMap<Core.Domain.OrderBookRow, OrderBookRow>();
            CreateMap<Core.Domain.OrderBook, OrderBook>();
            CreateMap<Core.Domain.Arbitrage, Arbitrage>();
        }
    }
}
