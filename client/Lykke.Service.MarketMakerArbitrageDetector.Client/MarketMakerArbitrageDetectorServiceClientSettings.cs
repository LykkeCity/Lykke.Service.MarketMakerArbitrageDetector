using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.MarketMakerArbitrageDetector.Client 
{
    /// <summary>
    /// MarketMakerArbitrageDetector client settings.
    /// </summary>
    public class MarketMakerArbitrageDetectorServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
