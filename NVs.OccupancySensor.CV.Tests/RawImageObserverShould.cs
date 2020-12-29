using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Castle.Core.Logging;
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
    public sealed class RawImageObserverShould
    {
        private readonly Mock<ILogger<RawImageObserver>> logger;

        public RawImageObserverShould()
        {
            logger = new Mock<ILogger<RawImageObserver>>();
        }

        [Fact]
        public async Task ReturnImageOnceItWasObserved()
        {
            var expectedFrame = new Mat(new Size(100, 100), DepthType.Cv32F, 3);
            Image<Rgb, int> expectedImage = expectedFrame.ToImage<Rgb, int>();
            var expectedJpeg = expectedImage.ToJpegData();
            
            var observer = new RawImageObserver(logger.Object);
            
            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnNext(expectedImage);
            });

            var image = await observer.GetImage();
            var actualJpeg = image.ToJpegData();
            Assert.Equal(expectedJpeg, actualJpeg);
        }

        [Fact]
        public async Task ThrowIOExceptionOnError()
        {
            var observer = new RawImageObserver(logger.Object);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnError(new TestException());
            });

            await Assert.ThrowsAsync<IOException>(async () =>
            {
                await observer.GetImage();
            });
        }
        
        [Fact]
        public async Task ThrowInvalidOperationExceptionIfNullMatReceivedFromObservable()
        {
            var observer = new RawImageObserver(logger.Object);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnNext(null);
            });

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await observer.GetImage();
            });
        }

        [Fact]
        public async Task LogErrorIfNullMatReceivedFromObservable()
        {
            logger
                .Setup(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>((x, _)=> x.ToString().Contains("null image received")),
                        It.Is<Exception>((e, _) => e == null),
                        It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");
            var observer = new RawImageObserver(logger.Object);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnNext(null);
            });

            try
            {
                await observer.GetImage();
            }
            catch (Exception)
            {
                // ignored - it's expected that GetImage will throw an exception, but this test checks that logger was called before that
            }
            
            logger.Verify();
        }

        [Fact]
        public async Task LogErrorReceivedOnError()
        {
            logger
                .Setup(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                        It.IsAny<TestException>(),
                        It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");
            var observer = new RawImageObserver(logger.Object);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnError(new TestException());
            });

            try
            {
                await observer.GetImage();
            }
            catch (Exception)
            {
                // ignored - it's expected that GetImage will throw an exception, but this test checks that logger was called before that
            }

            logger.Verify();
        }

        [Fact]
        public async Task ReturnNullIfOnCompletedReceived()
        {
            var observer = new RawImageObserver(logger.Object);

            var _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(110));
                observer.OnCompleted();
            });

            var result = await observer.GetImage();
            Assert.Null(result);
        }
    }
}