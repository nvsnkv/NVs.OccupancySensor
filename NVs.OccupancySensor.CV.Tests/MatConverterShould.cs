using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
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
            Mat createFrame() => new Mat(new Size(100, 100), DepthType.Cv32F, 3);

            var expectedFrame = createFrame();
            var expectedImage = expectedFrame.ToImage<Rgb,int>().ToJpegData();
            
            var frame = createFrame();
            var converter = new MatConverter(logger.Object);

            var actualImage = converter.Convert(frame).ToJpegData();

            Assert.Equal(expectedImage, actualImage);
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