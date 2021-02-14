using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class DenoisingStreamShould
    {
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        private readonly Mock<IDenoisingStrategy> strategy = new Mock<IDenoisingStrategy>();

        [Fact]
        public async Task ProvideDataForObservers()
        {
            var inputImage = new Image<Rgb, byte>(1, 1);
            var expectedImage = new Image<Rgb, byte>(10, 10);
            strategy.Setup(s => s.Denoise(inputImage)).Returns(expectedImage);

            var denoiser = new DenoisingStream(strategy.Object, CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Subscribe(observer))
            {
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(1, observer.ReceivedItems.Count);
            Assert.Equal(expectedImage, observer.ReceivedItems.Keys.First());
        }

        [Fact]
        public async Task DropNewFramesIfPreviousOneIsStillInProgress()
        {
            var inputImage = new Image<Rgb, byte>(1, 1);
            var expectedImage = new Image<Rgb, byte>(10, 10);
            strategy.Setup(s => s.Denoise(inputImage)).Returns(() => {
                Task.Delay(TimeSpan.FromMilliseconds(900)).Wait();
                return expectedImage;
            });

            var denoiser = new DenoisingStream(strategy.Object, CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Subscribe(observer))
            {
                var _ =Task.Run(() => denoiser.Process(inputImage));
                _ = Task.Run(() => denoiser.Process(inputImage));
                _ = Task.Run(() => denoiser.Process(inputImage));
                _ = Task.Run(() => denoiser.Process(inputImage));

                await Task.Delay(TimeSpan.FromMilliseconds(1300));
            }

            Assert.Equal(1, observer.ReceivedItems.Count);
        }

        [Fact]
        public async Task CompleteStreamWhenRequested()
        {
            var denoiser = new DenoisingStream(strategy.Object, CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using(denoiser.Subscribe(observer))
            {
                denoiser.Complete();
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task CompleteStreamIfErrorOccured()
        {
            var inputImage = new Image<Rgb, byte>(1, 1);
            strategy.Setup(s => s.Denoise(inputImage)).Throws<TestException>();
            var denoiser = new DenoisingStream(strategy.Object, CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using(denoiser.Subscribe(observer))
            {
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.IsType<TestException>(observer.Error);
            Assert.True(observer.StreamCompleted);
        }
    }
}