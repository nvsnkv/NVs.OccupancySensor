using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Transformation;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class TransformsShould
    {
        [Fact]
        public void NotResizeImageIfResizeFactorEqualsToOne()
        {
            var expectedImage = new Image<Gray, byte>(100, 100);
            var actual = Transforms.Resize(1)(expectedImage);
            
            Assert.Equal(expectedImage, actual);
        }

        [Fact]
        public void NotBlurIfKSizeIsZero()
        {
            var expectedImage = new Image<Gray, byte>(100, 100);
            var actual = Transforms.MedianBlur(0)(expectedImage);
            
            Assert.Equal(expectedImage, actual);
        }

        [Fact]
        public void ResizeInAccordanceWithResizeFactor()
        {
            var expectedImage = new Image<Gray, byte>(200, 100);
            var actual = Transforms.Resize(0.5d)(expectedImage);
            
            Assert.NotEqual(expectedImage, actual);
            Assert.Equal(100, actual.Width);
            Assert.Equal(50, actual.Height);
        }
        
        // no idea how to test blur :)
    }
}