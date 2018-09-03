# Lykke.Service.MarketMakerArbitrageDetector
Arbitrage Detector service for detecting arbitrages between Market Makers.

Client: [Nuget](https://www.nuget.org/packages/Lykke.Service.MarketMakerArbitrageDetector.Client)

# Client usage

Register client service in container:

```cs
ContainerBuilder.RegisterMarketMakerArbitrageDetectorClient({urlToService}, null);
```