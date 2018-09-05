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
    public class ArbitragesController : Controller, IArbitragesApi
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public ArbitragesController(IArbitrageDetectorService arbitrageDetectorService)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        [HttpGet]
        [SwaggerOperation("ArbitragesGetAll")]
        [ProducesResponseType(typeof(IReadOnlyList<Arbitrage>), (int)HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<Arbitrage>> GetAllAsync(string target, string source)
        {
            var domain = _arbitrageDetectorService.GetAll(target, source);
            var model = Mapper.Map<List<Arbitrage>>(domain);

            return Task.FromResult((IReadOnlyCollection<Arbitrage>)model);
        }
    }
}
