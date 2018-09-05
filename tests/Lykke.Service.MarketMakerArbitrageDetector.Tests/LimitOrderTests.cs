using Lykke.Service.MarketMakerArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.MarketMakerArbitrageDetector.Tests
{
    public class VolumePriceTests
    {
        [Fact]
        public void ReciprocalTest()
        {
            var volumePrice = new LimitOrder(null, null, 8999.95m, 7);
            var reciprocal = volumePrice.Reciprocal();

            Assert.Equal(reciprocal.Price, 1 / 8999.95m);
            Assert.Equal(reciprocal.Volume, 8999.95m * 7);
        }
    }
}
