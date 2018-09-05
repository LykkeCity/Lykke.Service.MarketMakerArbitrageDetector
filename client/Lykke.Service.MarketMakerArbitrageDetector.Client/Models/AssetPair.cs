namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an asset pair.
    /// </summary>
    public class AssetPair
    {
        /// <summary>
        /// Unique identifier of the asset pair.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the asset pair.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Base asset.
        /// </summary>
        public Asset Base { get; set; }

        /// <summary>
        /// Quote asset.
        /// </summary>
        public Asset Quote { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
