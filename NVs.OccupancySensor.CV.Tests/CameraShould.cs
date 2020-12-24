using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
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
            videoMock = new Mock<VideoCapture>(MockBehavior.Strict, 0, VideoCapture.API.Any);
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());
            loggerMock = new Mock<ILogger<Camera>>();
        }

        [Fact]
        public async Task ProvideDataForObserver()
        {
            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object, TimeSpan.FromMilliseconds(10));
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
            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object, TimeSpan.FromMilliseconds(10));
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
            var observers = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => new TestMatObserver()).ToList();

            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), loggerMock.Object, TimeSpan.FromMilliseconds(10));
            var unsubscribers = observers.Select(o => camera.Subscribe(o)).ToList();

            await Task.Delay(2000);

            foreach (var unsubscriber in unsubscribers)
            {
                unsubscriber.Dispose();
            }
            
            Assert.True(observers[0].ReceivedItems.Count > 1);
            Assert.Equal(observers[0].ReceivedItems.Count, observers[1].ReceivedItems.Count);
            Assert.Equal(observers[1].ReceivedItems.Count, observers[2].ReceivedItems.Count);
            Assert.Equal(observers[3].ReceivedItems.Count, observers[4].ReceivedItems.Count);

            foreach (var mat in observers[0].ReceivedItems.Keys)
            {
                Assert.True(observers[0].ReceivedItems[mat] - observers[1].ReceivedItems[mat] < TimeSpan.FromMilliseconds(10));
                Assert.True(observers[0].ReceivedItems[mat] - observers[2].ReceivedItems[mat] < TimeSpan.FromMilliseconds(10));
                Assert.True(observers[0].ReceivedItems[mat] - observers[3].ReceivedItems[mat] < TimeSpan.FromMilliseconds(10));
            }
        }
    }
}
