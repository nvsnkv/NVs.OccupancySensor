using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Correction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class BypassCorrectionStrategyShould
    {
        private readonly BypassCorrectionStrategy strategy = new();
        [Fact]
        public void NotChangeInputImage()
        {
            var expected = new Image<Gray, byte>(10, 10);
            var actual = strategy.Apply(expected);

            Assert.Same(expected, actual);
        }
    }
}