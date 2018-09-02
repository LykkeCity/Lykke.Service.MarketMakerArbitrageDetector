using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.Client.Api;
using Lykke.Service.MarketMakerArbitrageDetector.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using ClientSettings = Lykke.Service.MarketMakerArbitrageDetector.Client.Models.Settings;
using DomainSettings = Lykke.Service.MarketMakerArbitrageDetector.Core.Domain.Settings;

namespace Lykke.Service.MarketMakerArbitrageDetector.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        [SwaggerOperation("SettingsGet")]
        [ProducesResponseType(typeof(ClientSettings), (int)HttpStatusCode.OK)]
        public async Task<ClientSettings> GetAsync()
        {
            var domain = await _settingsService.GetAsync();
            var model = Mapper.Map<ClientSettings>(domain);

            return model;
        }

        [HttpPost]
        [SwaggerOperation("SettingsSet")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task SetAsync([FromBody]ClientSettings settings)
        {
            var domain = Mapper.Map<DomainSettings>(settings);
            await _settingsService.SetAsync(domain);
        }
    }
}
