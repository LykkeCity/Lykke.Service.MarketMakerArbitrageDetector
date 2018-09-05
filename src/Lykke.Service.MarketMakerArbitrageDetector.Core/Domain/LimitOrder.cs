using System;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class LimitOrder : IComparable<LimitOrder>
    {
        public string OrderId { get; }

        public string WalletId { get; }

        public decimal Price { get; }

        public decimal Volume { get; set; }


        public LimitOrder(string orderId, string walletId, decimal price, decimal volume)
        {
            OrderId = orderId;
            WalletId = walletId;
            Price = price;
            Volume = volume;
        }

        public LimitOrder(decimal price, decimal volume)
        {
            Price = price;
            Volume = volume;
        }

        public LimitOrder Reciprocal()
        {
            return new LimitOrder(OrderId, WalletId, 1 / Price, Volume * Price);
        }

        public LimitOrder Clone()
        {
            return new LimitOrder(OrderId, WalletId, Price, Volume);
        }

        public LimitOrder CloneWithoutIds()
        {
            return new LimitOrder(Price, Volume);
        }

        public int CompareTo(LimitOrder other)
        {
            return Price.CompareTo(other.Price);
        }

        public override string ToString()
        {
            return $"Price: {Price}, Volume: {Volume}, OrderId: {OrderId}, WalletId: {WalletId}";
        }
    }
}
