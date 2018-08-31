using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class AssetPairTests
    {
        private readonly Asset _btc = new Asset("BTC", "BTC");
        private readonly Asset _usd = new Asset("USD", "USD");
        private readonly AssetPair _btcusd = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"), 8, 8);

        [Fact]
        public void AssetPairInvertTest()
        {
            var inverted = _btcusd.Invert();

            Assert.Equal(_btc.Id, inverted.Quote.Id);
            Assert.Equal(_usd.Id, inverted.Base.Id);
        }

        [Fact]
        public void AssetPairIsInvertedTest()
        {
            var inverted = _btcusd.Invert();

            Assert.True(_btcusd.IsInverted(inverted));
            Assert.True(inverted.IsInverted(_btcusd));
        }

        [Fact]
        public void AssetPairIsEqualTest()
        {
            var equalAssetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"), 8, 8);

            Assert.True(_btcusd.Equals(equalAssetPair));
            Assert.True(equalAssetPair.Equals(_btcusd));
        }

        [Fact]
        public void AssetPairIsEqualOrInvertedTest()
        {
            var equalAssetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"), 8, 8);
            var inverted = _btcusd.Invert();

            Assert.True(_btcusd.EqualOrInverted(equalAssetPair));
            Assert.True(equalAssetPair.EqualOrInverted(_btcusd));
            Assert.True(_btcusd.EqualOrInverted(inverted));
            Assert.True(inverted.EqualOrInverted(_btcusd));
        }

        [Fact]
        public void AssetPairContainsTest()
        {
            Assert.True(_btcusd.ContainsAsset("BTC"));
            Assert.True(_btcusd.ContainsAsset("USD"));
            Assert.False(_btcusd.ContainsAsset("EUR"));
        }
    }
}
