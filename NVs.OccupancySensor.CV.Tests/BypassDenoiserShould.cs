using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class BypassDenoiserShould
    {
        [Fact]
        public void BypassImageAsIs()
        {
            var expectedImage = new Image<Rgb, byte>(1, 1);
            var actual = new BypassDenoiser().Denoise(expectedImage);

            Assert.Equal(expectedImage, actual);
        }
    }
}