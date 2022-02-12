using System;
using System.Collections.Generic;
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
using NVs.OccupancySensor.CV.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class DenoisingStreamShould
    {
        private readonly Mock<ILogger> logger = new();
        private readonly Mock<IDenoisingStrategy> strategy = new();
        
        [Fact]
        public async Task ProvideDataForObservers()
        {
            var inputImage = new Image<Gray, byte>(1, 1);
            var expectedImages = new List<Image<Gray, byte>>();
            var i = 0;

            strategy.Setup(s => s.Denoise(inputImage)).Returns(() =>
            {
                var value = new Image<Gray, byte>(++i, 1);
                expectedImages.Add(value);
                return value;
            });

            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Subscribe(observer))
            {
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(2, observer.ReceivedItems.Count);
            Assert.Equal(expectedImages[0], observer.ReceivedItems.Keys.First());
            Assert.Equal(expectedImages[1], observer.ReceivedItems.Keys.Skip(1).First());
        }

        [Fact]
        public async Task NotProvideDataForObserversAfterCompletion()
        {
            var inputImage = new Image<Gray, byte>(1, 1);
            var expectedImage = new Image<Gray, byte>(10, 10);
            strategy.Setup(s => s.Denoise(inputImage)).Returns(expectedImage);

            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Subscribe(observer))
            {
                denoiser.Complete();
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Empty(observer.ReceivedItems);
        }

        [Fact]
        public async Task DropNewFramesIfPreviousOneIsStillInProgress()
        {
            var inputImage = new Image<Gray, byte>(1, 1);
            var expectedImage = new Image<Gray, byte>(10, 10);
            strategy.Setup(s => s.Denoise(inputImage)).Returns(() => {
                Task.Delay(TimeSpan.FromMilliseconds(900)).Wait();
                return expectedImage;
            });

            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Subscribe(observer))
            {
                var _ =Task.Run(() => denoiser.Process(inputImage));
                var __ = Task.Run(() => denoiser.Process(inputImage));
                var ___ = Task.Run(() => denoiser.Process(inputImage));
                var _____ = Task.Run(() => denoiser.Process(inputImage));

                await Task.Delay(TimeSpan.FromMilliseconds(1300));
            }

            Assert.Single(observer.ReceivedItems);
        }

        [Fact]
        public async Task CompleteStreamWhenRequested()
        {
            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using(denoiser.Subscribe(observer))
            {
                denoiser.Complete();
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task CompleteStreamIfErrorOccurred()
        {
            var inputImage = new Image<Gray, byte>(1, 1);
            strategy.Setup(s => s.Denoise(inputImage)).Throws<TestException>();
            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            var observer = new TestImageObserver();

            using(denoiser.Subscribe(observer))
            {
                denoiser.Process(inputImage);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.IsType<TestException>(observer.Error);
            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public void BeNotCompletedByDefault()
        {
            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            Assert.False(denoiser.Completed);
        }

        [Fact]
        public void BeCompletedAfterCompletion()
        {
            var denoiser = new DenoisingStream(strategy.Object, new Counter(), CancellationToken.None, logger.Object);
            denoiser.Complete();

            Assert.True(denoiser.Completed);
        }
    }
}