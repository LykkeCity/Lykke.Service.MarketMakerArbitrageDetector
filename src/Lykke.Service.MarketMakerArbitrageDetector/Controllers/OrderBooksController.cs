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
        private readonly ISettingsService _settingsService;

        public OrderBooksController(IOrderBooksService orderBooksService, ISettingsService settingsService)
        {
            _orderBooksService = orderBooksService;
            _settingsService = settingsService;
        }

        [HttpGet]
        [SwaggerOperation("OrderBooksGetAllRaws")]
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
        public async Task<OrderBook> GetAsync(string assetPairId)
        {
            var domain = _orderBooksService.Get(assetPairId);
            var model = Mapper.Map<OrderBook>(domain);

            // Map wallets names
            var wallets = (await _settingsService.GetAsync()).Wallets;
            foreach (var ask in model.Asks)
                if (wallets.ContainsKey(ask.WalletId))
                    ask.WalletName = wallets[ask.WalletId];
            foreach (var bid in model.Bids)
                if (wallets.ContainsKey(bid.WalletId))
                    bid.WalletName = wallets[bid.WalletId];

            return model;
        }
    }
}
