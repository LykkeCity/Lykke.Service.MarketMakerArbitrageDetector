using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Models;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

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
        [SwaggerOperation("OrderBooksGetAll")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderBookRow>), (int)HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<OrderBookRow>> GetAllRowsAsync(bool wantedOnly = true)
        {
            var domain = _orderBooksService.GetAllRows(wantedOnly);
            var model = Mapper.Map<List<OrderBookRow>>(domain);

            return Task.FromResult((IReadOnlyCollection<OrderBookRow>)model);
        }

        [HttpGet("{assetPairId}")]
        [SwaggerOperation("OrderBooksGet")]
        [ProducesResponseType(typeof(OrderBook), (int)HttpStatusCode.OK)]
        public Task<OrderBook> GetAsync(string assetPairId)
        {
            var domain = _orderBooksService.Get(assetPairId);
            var model = Mapper.Map<OrderBook>(domain);

            return Task.FromResult(model);
        }
    }
}
