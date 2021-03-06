﻿using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using System;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client
{
    /// <summary>
    /// Autofac extension for client registration.
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="IMarketMakerArbitrageDetectorClient"/> in Autofac container using <see cref="MarketMakerArbitrageDetectorServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">MarketMakerArbitrageDetector client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterMarketMakerArbitrageDetectorClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] MarketMakerArbitrageDetectorServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", $"{nameof(settings.ServiceUrl)}");

            var clientBuilder = HttpClientGenerator.HttpClientGenerator.BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new MarketMakerArbitrageDetectorClient(clientBuilder.Create()))
                .As<IMarketMakerArbitrageDetectorClient>()
                .SingleInstance();
        }
    }
}
