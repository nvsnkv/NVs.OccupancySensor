using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
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
        public async Task InvokeSubscribersInParallel()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());
            var observers = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => new HeavyTestMatObserver())
                .ToList();

            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10));
            var unsubscribers = observers.Select(o => camera.Subscribe(o)).ToList();

            await Task.Delay(5000);

            foreach (var unsubscriber in unsubscribers)
            {
                unsubscriber.Dispose();
            }

            Assert.True(observers[0].ReceivedItems.Count > 1);
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                {
                    Assert.Equal(observers[0].ReceivedItems.Count, observers[1].ReceivedItems.Count);
                }

            }

            foreach (var mat in observers[0].ReceivedItems.Keys)
            {
                for (var i = 1; i < Environment.ProcessorCount; i++)
                {
                    Assert.True(observers[0].ReceivedItems[mat] - observers[1].ReceivedItems[mat] <
                                TimeSpan.FromMilliseconds(10));
                }
            }
        }

        [Fact]
        public async Task LogErrors()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<InvalidOperationException>();
            loggerMock
                .Setup(
                    l => l.Log(
                    LogLevel.Error, 
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                    It.IsAny<InvalidOperationException>(),
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
            videoMock.Setup(v => v.QueryFrame()).Throws<InvalidOperationException>();
            var observer = new TestMatObserver();

            using (new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.NotNull(observer.Error);
            Assert.IsType<InvalidOperationException>(observer.Error);
        }

        [Fact]
        public async Task CompletesStreamOnError()
        {
            videoMock.Setup(v => v.QueryFrame()).Throws<InvalidOperationException>();
            var observer = new TestMatObserver();

            using (new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object,
                TimeSpan.FromMilliseconds(10)).Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            
            Assert.True(observer.StreamCompleted);
        }
    }
}
