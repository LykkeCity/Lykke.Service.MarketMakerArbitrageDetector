using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models.OrderBooks;

namespace Lykke.Service.MarketMakerArbitrageDetector
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<OrderBook, Core.Domain.OrderBooks.OrderBook>();
            CreateMap<Core.Domain.OrderBooks.OrderBook, OrderBook>();
        }
    }
}
