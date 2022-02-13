using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class BackgroundSubtractorShould : StageShould
    {
        private readonly Mock<ISubtractionStrategy> strategy = null!;
        private readonly BackgroundSubtractor subtractor;


        public BackgroundSubtractorShould() 
            : this(new Mock<IBackgroundSubtractorFactory>(), new Mock<ISubtractionStrategy>(), new Mock<IBackgroundSubtractorSettings>(), new Mock<ILogger<BackgroundSubtractor>>()) { }
        internal BackgroundSubtractorShould(Mock<IBackgroundSubtractorFactory> factory, Mock<ISubtractionStrategy> strategy, Mock<IBackgroundSubtractorSettings> settings, Mock<ILogger<BackgroundSubtractor>> logger) 
            : this(CreateSubtractor(factory, strategy, settings, logger))
        {
            this.strategy = strategy;
        }

        private BackgroundSubtractorShould(BackgroundSubtractor subtractor) : base(subtractor)
        {
            this.subtractor = subtractor;
        }

        [Fact]
        public void ApplyStrategyToInputImage()
        {
            var image = new Image<Gray, byte>(1, 1);
            strategy.Setup(s => s.GetForegroundMask(image)).Returns(new Image<Gray, byte>(1, 1)).Verifiable("GetForegroundMask was not invoked!");
            subtractor.OnNext(image);

            strategy.Verify();
        }

        [Fact]
        public void ResetStrategyOnReset()
        {
            strategy.Setup(s => s.Reset()).Verifiable();
            subtractor.Reset();

            strategy.Verify(s => s.Reset());
        }

        private static BackgroundSubtractor CreateSubtractor(Mock<IBackgroundSubtractorFactory> factory, Mock<ISubtractionStrategy> strategy, Mock<IBackgroundSubtractorSettings> settings, Mock<ILogger<BackgroundSubtractor>> logger)
        {
            strategy.Setup(s => s.GetForegroundMask(It.IsAny<Image<Gray, byte>>())).Returns(new Image<Gray, byte>(1, 1));
            factory.Setup(f => f.Create(It.IsAny<string>())).Returns(strategy.Object);
            return new BackgroundSubtractor(factory.Object, settings.Object, logger.Object);
        }

        protected override void SetupLongRunningPayload(TimeSpan delay)
        {
            strategy.Setup(s => s.GetForegroundMask(It.IsAny<Image<Gray, byte>>())).Returns(() =>
            {
                Task.Delay(delay).Wait();
                return new Image<Gray, byte>(1, 1);
            });
        }
    }
}