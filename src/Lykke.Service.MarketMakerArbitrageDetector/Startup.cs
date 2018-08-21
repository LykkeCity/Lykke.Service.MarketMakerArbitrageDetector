using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.MarketMakerArbitrageDetector.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using AutoMapper;
using Lykke.Service.MarketMakerArbitrageDetector.AzureRepositories;

namespace Lykke.Service.MarketMakerArbitrageDetector
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "MarketMakerArbitrageDetector API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "MarketMakerArbitrageDetectorLog";
                    logs.AzureTableConnectionStringResolver = settings =>
                        settings.MarketMakerArbitrageDetectorService.Db.LogsConnectionString;
                };

                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                });
                Mapper.AssertConfigurationIsValid();
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });
        }
    }
}
