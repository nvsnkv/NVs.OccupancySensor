using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class BackgroundSubtractorShould
    {
        private readonly Mock<IBackgroundSubtractorFactory> factory = new Mock<IBackgroundSubtractorFactory>();
        private readonly Mock<ISubtractionStrategy> strategy = new Mock<ISubtractionStrategy>();
        private readonly Mock<IBackgroundSubtractorSettings> settings = new Mock<IBackgroundSubtractorSettings>();
        private readonly Mock<ILogger<BackgroundSubtractor>> logger = new Mock<ILogger<BackgroundSubtractor>>();
        private readonly BackgroundSubtractor subtractor;

        public BackgroundSubtractorShould()
        {
            strategy.Setup(s => s.GetForegroundMask(It.IsAny<Image<Rgb, byte>>())).Returns(new Image<Gray, byte>(1, 1));
            factory.Setup(f => f.Create(It.IsAny<string>())).Returns(strategy.Object);
            subtractor = new BackgroundSubtractor(factory.Object, settings.Object, logger.Object);
        }

        [Fact]
        public void ApplyStrategyToInputImage()
        {
            var image = new Image<Rgb, byte>(1, 1);
            strategy.Setup(s => s.GetForegroundMask(image)).Returns(new Image<Gray, byte>(1, 1)).Verifiable("GetForegroundMask was not invoked!");
            subtractor.OnNext(image);

            strategy.Verify();
        }

        [Fact]
        public async Task CompleteOutputStreamWhenSourceStreamCompleted()
        {

            var observer = new TestImageObserver<Gray>();
            
            using (subtractor.Output.Subscribe(observer))
            {
                await Task.Run(() => subtractor.OnCompleted());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task CompleteStreamOnReset()
        {
            
            var observer = new TestImageObserver<Gray>();

            using (subtractor.Output.Subscribe(observer))
            {
                await Task.Run(() => subtractor.Reset());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task ForwardErrors()
        {
            
            var observer = new TestImageObserver<Gray>();
            
            using (subtractor.Output.Subscribe(observer))
            {
                await Task.Run(() => subtractor.OnError(new TestException()));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.IsType<TestException>(observer.Error);
        }
    }
}