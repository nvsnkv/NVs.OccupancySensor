using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{

    public sealed class ForegroundMaskBasedPeopleDetectorShould
    {
        private readonly Mock<IDecisionMaker> decisionMaker = new Mock<IDecisionMaker>();
        private readonly Mock<ILogger<ForegroundMaskBasedPeopleDetector>> logger = new Mock<ILogger<ForegroundMaskBasedPeopleDetector>>();

        [Fact]
        public void NotifyWhenPeopleDetected()
        {
            decisionMaker.Setup(m => m.PresenceDetected(It.IsAny<Image<Gray, byte>>())).Returns(true);

            var propertyName = string.Empty;
            var detector = new ForegroundMaskBasedPeopleDetector(decisionMaker.Object ,logger.Object);
            detector.PropertyChanged += (_, e) => propertyName = e.PropertyName;
            
            detector.OnNext(new Image<Gray, byte>(1, 1));

            Assert.Equal(nameof(IPeopleDetector.PeopleDetected), propertyName);
        }

        [Fact]
        public void NotifyWhenPeopleNotDetected()
        {
            decisionMaker.Setup(m => m.PresenceDetected(It.IsAny<Image<Gray, byte>>())).Returns(false);

            var propertyName = string.Empty;
            var detector = new ForegroundMaskBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.PropertyChanged += (_, e) => propertyName = e.PropertyName;
            
            detector.OnNext(new Image<Gray, byte>(1, 1));

            Assert.Equal(nameof(IPeopleDetector.PeopleDetected), propertyName);
        }
     
        [Fact]
        public void SetPeopleDetectedToNullWhenStreamEnds()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullWhenStreamErrorsOut()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullOnReset()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.PeopleDetected);
        }
    }
}