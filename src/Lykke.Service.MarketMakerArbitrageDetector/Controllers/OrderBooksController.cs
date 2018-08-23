using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.MarketMakerArbitrageDetector.Controllers
{
    [Route("/api/[controller]")]
    public class OrderBooksController : Controller, IOrderBooksApi
    {
        private readonly IOrderBooksService _orderBooksService;

        public OrderBooksController(IOrderBooksService orderBooksService)
        {
            _orderBooksService = orderBooksService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<OrderBook>), (int)HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<OrderBook>> GetAllAsync()
        {
            var domain = _orderBooksService.GetAll();
            var model = Mapper.Map<List<OrderBook>>(domain);
            return Task.FromResult((IReadOnlyCollection<OrderBook>)model);
        }
    }
}
