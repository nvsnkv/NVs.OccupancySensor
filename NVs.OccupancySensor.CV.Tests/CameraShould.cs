using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class CameraShould
    {
        private readonly Mock<VideoCapture> videoMock;
        private readonly Mock<ILogger<Camera>> loggerMock;

        public CameraShould()
        {
            videoMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
            loggerMock = new Mock<ILogger<Camera>>(MockBehavior.Loose);
        }

        [Fact]
        public async Task ProvideDataForObserver()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());
            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestMatObserver();

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
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());
            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestMatObserver();

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

            using (new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(new TestMatObserver()))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            loggerMock.Verify();
        }

        [Fact]
        public async Task NotifyObserversAboutErrors()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<TestException>();
            var observer = new TestMatObserver();

            using (new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
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
            var observer = new TestMatObserver();

            using (new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task CompleteTheStreamIfCancellationRequested()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());

            var cts = new CancellationTokenSource();
            var camera = new Camera(videoMock.Object, cts, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestMatObserver();

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
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());

            var cts = new CancellationTokenSource();
            var camera = new Camera(videoMock.Object, cts, loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var observer = new TestMatObserver();

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
                    Task.Delay(TimeSpan.MaxValue).Wait(cts.Token);
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
                
                return new Mat();
            });

            var observersCount = Environment.ProcessorCount - 2;
            var observers = Enumerable.Range(0, observersCount)
                .Select(_ => new HeavyTestMatObserver(TimeSpan.FromMilliseconds(200)))
                .ToList();

            var camera = new Camera(videoMock.Object, cts, loggerMock.Object, TimeSpan.FromMilliseconds(100));
            
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
