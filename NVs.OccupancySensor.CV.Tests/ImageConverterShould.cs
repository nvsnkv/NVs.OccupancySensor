using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed  class ImageConverterShould
    {
        private readonly Mock<ILogger<ImageConverter>> logger = new Mock<ILogger<ImageConverter>>();
        
        [Theory]
        [InlineData(1920,1080)]
        [InlineData(1920,200)]
        [InlineData(50,600)]
        public void ResizeValidImageToTargetSize(int originalWidth, int originalHeight)
        {
            var expectedWidth = 640;
            var expectedHeight = 360;
            var settings = new ConversionSettings(new Size(expectedWidth, expectedHeight), false, 0);
            
            var image = new Image<Rgb,byte>(originalWidth, originalHeight, new Rgb(Color.Aqua));
            var resizer = new ImageConverter(settings, logger.Object);

            var result = resizer.Convert(image);
            Assert.Equal(expectedWidth, result.Width);
            Assert.Equal(expectedHeight, result.Height);
        }

        [Fact]
        public void ThrowArgumentNullExceptionIfInputIsNull()
        {
            var resizer = new ImageConverter(ConversionSettings.Default, logger.Object);
            Assert.Throws<ArgumentNullException>(() => resizer.Convert(null!));
        }
        
        [Fact]
        public void ThrowArgumentNullExceptionIfSettingsAreNull()
        {
            var resizer = new ImageConverter(ConversionSettings.Default, logger.Object);
            Assert.Throws<ArgumentNullException>(() => resizer.Settings =null!);
            Assert.Throws<ArgumentNullException>(() => new ImageConverter(null, logger.Object));
        }

        [Theory]
        [InlineData(640, 360)]
        [InlineData(640, 200)]
        [InlineData(200, 360)]
        [InlineData(200, 200)]
        public void BypassImageIfItsLessThenTargetSizeAndGrayScaleConversionIsDisabled(int originalWidth, int originalHeight)
        {
            var expectedWidth = 640;
            var expectedHeight = 360;
            var settings = new ConversionSettings(new Size(expectedWidth, expectedHeight), false, 0);

            var image = new Image<Rgb,byte>(originalWidth, originalHeight, new Rgb(Color.Aqua));
            var resizer = new ImageConverter(settings, logger.Object);

            var result = resizer.Convert(image);
            Assert.Same(image,result);
        }

        [Fact]
        public void ConvertImageToGrayscaleIfGrayScaleConversionIsEnabled()
        {
            var input = new Image<Rgb,byte>(100, 100, new Rgb(Color.Red));
            var expectedResult = new Image<Rgb,byte>(100, 100, new Rgb(76.2449951, 76.2449951, 76.2449951));

            var converter = new ImageConverter(new ConversionSettings(null, true, 0), logger.Object);
            var actualResult = converter.Convert(input);

            var color = actualResult[0,0];
            Assert.Equal(color.Red, color.Blue);
            Assert.Equal(color.Red, color.Green);
            Assert.True(color.Red > 75);
            Assert.True(color.Red < 100);

            for(var i=0; i<100; i++)
            for(var j=0; j<100; j++)
            Assert.True(color.Equals(actualResult[i,j]));
        }
        
        [Fact]
        public void RotateImageIfRotationAngleIsNonZero()
        {
            var input = new Image<Rgb,byte>(100, 200, new Rgb(Color.Red));
            
            var converter = new ImageConverter(new ConversionSettings(null, true, 90), logger.Object);
            var actualResult = converter.Convert(input);

            Assert.Equal(input.Height, actualResult.Width);
            Assert.Equal(input.Width, actualResult.Height);
        }
    }
}