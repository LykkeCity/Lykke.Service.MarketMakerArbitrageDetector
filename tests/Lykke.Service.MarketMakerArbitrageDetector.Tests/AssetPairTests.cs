using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class AssetPairTests
    {
        private readonly Asset _btc = new Asset("BTC", "BTC");
        private readonly Asset _usd = new Asset("USD", "USD");
        private readonly AssetPair _assetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));

        [Fact]
        public void AssetPairReverseTest()
        {
            var reversed = _assetPair.Reverse();

            Assert.Equal(_btc.Id, reversed.Quote.Id);
            Assert.Equal(_usd.Id, reversed.Base.Id);
        }

        [Fact]
        public void AssetPairIsReversedTest()
        {
            var reversed = _assetPair.Reverse();

            Assert.True(_assetPair.IsReversed(reversed));
            Assert.True(reversed.IsReversed(_assetPair));
        }

        [Fact]
        public void AssetPairIsEqualTest()
        {
            var equalAssetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));

            Assert.True(_assetPair.Equals(equalAssetPair));
            Assert.True(equalAssetPair.Equals(_assetPair));
        }

        [Fact]
        public void AssetPairIsEqualOrReversedTest()
        {
            var equalAssetPair = new AssetPair("BTCUSD", "BTCUSD", new Asset("BTC", "BTC"), new Asset("USD", "USD"));
            var reversed = _assetPair.Reverse();

            Assert.True(_assetPair.EqualOrReversed(equalAssetPair));
            Assert.True(equalAssetPair.EqualOrReversed(_assetPair));
            Assert.True(_assetPair.EqualOrReversed(reversed));
            Assert.True(reversed.EqualOrReversed(_assetPair));
        }

        [Fact]
        public void AssetPairContainsTest()
        {
            Assert.True(_assetPair.ContainsAsset("BTC"));
            Assert.True(_assetPair.ContainsAsset("USD"));
            Assert.False(_assetPair.ContainsAsset("EUR"));
        }
    }
}
