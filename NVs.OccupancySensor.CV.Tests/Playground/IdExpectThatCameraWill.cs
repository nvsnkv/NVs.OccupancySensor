using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using Emgu.CV;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace NVs.OccupancySensor.CV.Tests.Playground
{
    public sealed class IdExpectThatCameraWill
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly Mock<VideoCapture> videoMock;

        public IdExpectThatCameraWill(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            videoMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
        }

      

        [Fact]
        public async Task InvokeSubscribersInParallel()
        {
            videoMock.Setup(v => v.QueryFrame()).Returns(() => new Mat());
            
            /* Here is the expected behaviour:
               QueryFrame cycle submits each 100 msecs. Every Observer "processes" frame in 1 second.
               Observers should be notified independently of each other (while system resources allows that)
            
               With that being said, I'd expect at least 6 frames to be sent by Camera within 2 seconds if we have amount of observers 4 times lesser than amount of processors
            */ 
            
            var observers = Enumerable.Range(0, (int)Math.Floor((double)Environment.ProcessorCount/4))
                .Select(_ => new HeavyTestMatObserver(TimeSpan.FromMilliseconds(1000)))
                .ToList();

            var logger = testOutputHelper.BuildLoggerFor<Camera>(new LoggingConfig
            {
                Formatter = new TestLogFormatter()
            });
            
            var camera = new Camera(videoMock.Object, new CancellationTokenSource(), logger,
                TimeSpan.FromMilliseconds(100));
            var unsubscribers = observers.Select(o => camera.Subscribe(o)).ToList();

            await Task.Delay(2000);

            foreach (var unsubscriber in unsubscribers)
            {
                unsubscriber.Dispose();
            }

            testOutputHelper.WriteLine($"frames received: {observers[0].ReceivedItems.Count}");
            Assert.True(observers[0].ReceivedItems.Count >= 6);
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
    }
}
