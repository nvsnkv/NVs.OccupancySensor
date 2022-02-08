using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class CameraStreamShould
    {
        private readonly Mock<VideoCapture> videoMock;
        private readonly Mock<ILogger<CameraStream>> loggerMock;

        public CameraStreamShould()
        {
            videoMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
            loggerMock = new Mock<ILogger<CameraStream>>(MockBehavior.Loose);
        }

        [Fact]
        public async Task ProvideDataForObserver()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);
            var camera = new CameraStream(videoMock.Object, CancellationToken.None, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestImageObserver();

            camera.Resume();

            var before = DateTime.Now;
            using (camera.Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            Assert.True(observer.ReceivedItems.Count > 50);
            Assert.DoesNotContain(observer.ReceivedItems, x => x.Value < before);
        }

        [Fact]
        public async Task NotProvideDataForUnsubscribedObservers()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);
            var camera = new CameraStream(videoMock.Object, CancellationToken.None, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestImageObserver();

            camera.Resume();

            using (camera.Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            var after = DateTime.Now;

            Assert.True(observer.ReceivedItems.Count > 0);
            Assert.DoesNotContain(observer.ReceivedItems, x => x.Value >= after);
        }

        [Fact]
        public async Task LogErrors()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<TestException>();
            loggerMock
                .Setup(
                    l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                    It.IsAny<TestException>(),
                    It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");

            var cameraStream = new CameraStream(videoMock.Object, CancellationToken.None, loggerMock.Object, TimeSpan.FromMilliseconds(10));
            using (cameraStream.Subscribe(new TestImageObserver()))
            {
                cameraStream.Resume();
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            loggerMock.Verify();
        }

        [Fact]
        public async Task NotifyObserversAboutErrors()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<TestException>();
            var observer = new TestImageObserver();

            using (new CameraStream(videoMock.Object, CancellationToken.None, loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.NotNull(observer.Error);
            Assert.IsType<TestException>(observer.Error);
        }

        [Fact]
        public async Task CompletesStreamOnError()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<TestException>();
            var observer = new TestImageObserver();

            using (new CameraStream(videoMock.Object, CancellationToken.None, loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task CompleteTheStreamIfCancellationRequested()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);

            var cts = new CancellationTokenSource();
            var camera = new CameraStream(videoMock.Object, cts.Token, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestImageObserver();

            camera.Subscribe(observer);

            // ReSharper disable MethodSupportsCancellation
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            cts.Cancel();

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            // ReSharper restore MethodSupportsCancellation

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task NotSendNewFramesIfCancellationRequested()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);

            var cts = new CancellationTokenSource();
            var camera = new CameraStream(videoMock.Object, cts.Token, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestImageObserver();

            camera.Subscribe(observer);

            // ReSharper disable MethodSupportsCancellation
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            cts.Cancel();
            var after = DateTime.Now;

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            // ReSharper restore MethodSupportsCancellation

            Assert.DoesNotContain(observer.ReceivedItems, x => x.Value > after);
        }

        [Fact]
        public async Task NotifyObserversIndependently()
        {
            var cts = new CancellationTokenSource();
            var invoked = false;
            videoMock.Setup(v => v.QueryFrame()).Returns(() =>
            {
                if (invoked)
                {
                    Task.Delay(TimeSpan.MaxValue, cts.Token).Wait(cts.Token);
                    return null;
                }

                invoked = true;
                
                // ReSharper disable twice MethodSupportsCancellation
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    cts.Cancel();
                });
                
                return new Image<Gray, byte>(100, 100).Mat;
            });

            var observersCount = Environment.ProcessorCount - 2;
            var observers = Enumerable.Range(0, observersCount)
                .Select(_ => new HeavyTestMatObserver(TimeSpan.FromMilliseconds(200)))
                .ToList();

            var camera = new CameraStream(videoMock.Object, cts.Token, loggerMock.Object, TimeSpan.FromMilliseconds(100));
            camera.Resume();

            foreach (var observer in observers)
            {
                camera.Subscribe(observer);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(300));

            for (var i = 1; i < observersCount; i++)
            {
                Assert.Single(observers[i].ReceivedItems);
                Assert.True(observers[0].ReceivedItems.First().Value - observers[i].ReceivedItems.First().Value < TimeSpan.FromMilliseconds(5));
            }
        }
    }
}
