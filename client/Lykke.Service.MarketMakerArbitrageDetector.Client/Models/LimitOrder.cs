namespace Lykke.Service.MarketMakerArbitrageDetector.Client.Models
{
    public class LimitOrder
    {
        public string OrderId { get; set; }

        public string WalletId { get; set; }

        public string WalletName { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }        

        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, WalletId: {WalletId}";
        }
    }
}
