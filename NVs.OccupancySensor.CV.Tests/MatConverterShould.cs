using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests 
{
    public sealed class MatConverterShould
    {
        private readonly Mock<ILogger<MatConverter>> logger;

        public MatConverterShould() 
        {
            logger = new Mock<ILogger<MatConverter>>(MockBehavior.Loose);
        }

        [Fact]
        public void ConvertValidMatObject()
        {
            Mat CreateFrame() => new Image<Rgb, int>(100, 100, new Rgb(Color.Black)).Mat.Clone();

            var expectedFrame = CreateFrame();
            var expectedImage = expectedFrame.ToImage<Rgb,int>();
            
            var frame = CreateFrame();
            var converter = new MatConverter(logger.Object);

            var actualImage = converter.Convert(frame);

            Assert.Equal(expectedImage.Size, actualImage.Size);
            var diff = expectedImage.AbsDiff(actualImage);

            Assert.True(diff.CountNonzero().All(i => i == 0));

        }

        [Fact]
        public void RethrowExceptionsHappenedDuringConversion() 
        {
            var converter = new MatConverter(logger.Object);

            // attempt to convert empty mat to image will throw ArgumnetException (as of Emgu.CV 4.4.0)
            Assert.Throws<ArgumentException>(() => converter.Convert(new Mat(Size.Empty, DepthType.Default, 1)));
        }

        [Fact]
        public void LogExceptionsHappenedDuringConversion() 
        {
            logger.Setup(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                        It.IsAny<ArgumentException>(),
                        It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");

            var converter = new MatConverter(logger.Object);

            // attempt to convert empty mat to image will throw ArgumnetException (as of Emgu.CV 4.4.0)
            try{ converter.Convert(new Mat(Size.Empty, DepthType.Default, 1)); } catch {};

            logger.Verify();
        }
    }
}