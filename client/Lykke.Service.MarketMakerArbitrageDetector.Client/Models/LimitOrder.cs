namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a limit order in a order book.
    /// </summary>
    public class LimitOrder
    {
        /// <summary>
        /// Unique identifier of the limit order.
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Unique identifier of the wallet of this order.
        /// </summary>
        public string WalletId { get; set; }

        /// <summary>
        /// Display name of the wallet. Can be empty.
        /// </summary>
        public string WalletName { get; set; }

        /// <summary>
        /// Price of the order.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Volume of the order.
        /// </summary>
        public decimal Volume { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, WalletId: {WalletId}";
        }
    }
}
