using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed  class ImageResizerShould
    {
        private readonly Mock<ILogger<ImageResizer>> logger = new Mock<ILogger<ImageResizer>>();
        private readonly Mock<IResizeSettings> settings = new Mock<IResizeSettings>();
        
        [Theory]
        [InlineData(1920,1080)]
        [InlineData(1920,200)]
        [InlineData(50,600)]
        public void ResizeValidImageToTargetSize(int originalWidth, int originalHeight)
        {
            var expectedWidth = 640;
            settings.SetupGet(s => s.TargetWidth).Returns(expectedWidth);
            var expectedHeight = 360;
            settings.SetupGet(s => s.TargetHeight).Returns(expectedHeight);

            var image = new Image<Rgb, float>(originalWidth, originalHeight, new Rgb(Color.Aqua));
            var resizer = new ImageResizer(settings.Object, logger.Object);

            var result = resizer.Resize(image);
            Assert.Equal(expectedWidth, result.Width);
            Assert.Equal(expectedHeight, result.Height);
        }

        [Fact]
        public void ThrowArgumentNullExceptionIfInputIsNull()
        {
            var resizer = new ImageResizer(settings.Object, logger.Object);
            Assert.Throws<ArgumentNullException>(() => resizer.Resize(null!));
        }
        
        [Fact]
        public void ThrowArgumentNullExceptionIfSettingsAreNull()
        {
            var resizer = new ImageResizer(settings.Object, logger.Object);
            Assert.Throws<ArgumentNullException>(() => resizer.Settings =null!);
            Assert.Throws<ArgumentNullException>(() => new ImageResizer(null, logger.Object));
        }

        [Theory]
        [InlineData(640, 360)]
        [InlineData(640, 200)]
        [InlineData(200, 360)]
        [InlineData(200, 200)]
        public void BypassImageIfItsLessThenTargetSize(int originalWidth, int originalHeight)
        {
            var expectedWidth = 640;
            settings.SetupGet(s => s.TargetWidth).Returns(expectedWidth);
            var expectedHeight = 360;
            settings.SetupGet(s => s.TargetHeight).Returns(expectedHeight);

            var image = new Image<Rgb, float>(originalWidth, originalHeight, new Rgb(Color.Aqua));
            var resizer = new ImageResizer(settings.Object, logger.Object);

            var result = resizer.Resize(image);
            Assert.Same(image,result);
        }
        
    }
}