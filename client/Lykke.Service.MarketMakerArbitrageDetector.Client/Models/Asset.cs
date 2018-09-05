namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an asset.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Unique identifier of the asset.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the asset.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
