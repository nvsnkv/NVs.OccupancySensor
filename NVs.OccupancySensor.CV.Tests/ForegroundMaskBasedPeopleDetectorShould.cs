using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{

    public sealed class ForegroundMaskBasedPeopleDetectorShould
    {
        private readonly Mock<IDecisionMaker> decisionMaker = new Mock<IDecisionMaker>();
        private readonly Mock<ILogger<CNTBackgroundSubtractionBasedPeopleDetector>> logger = new Mock<ILogger<CNTBackgroundSubtractionBasedPeopleDetector>>();

        [Fact]
        public void NotifyWhenPeopleDetected()
        {
            decisionMaker.Setup(m => m.PresenceDetected(It.IsAny<Image<Gray, byte>>())).Returns(true);

            var propertiesChanged = new List<string>();
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object ,logger.Object);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Gray, byte>(1, 1));

            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }

        [Fact]
        public void NotifyWhenPeopleNotDetected()
        {
            decisionMaker.Setup(m => m.PresenceDetected(It.IsAny<Image<Gray, byte>>())).Returns(false);

            var propertiesChanged = new List<string>();
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Gray, byte>(1, 1));
            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }
     
        [Fact]
        public void SetPeopleDetectedToNullWhenStreamEnds()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullWhenStreamErrorsOut()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullOnReset()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.PeopleDetected);
        }


        [Fact]
        public void RaisePropertyChangedWhenMaskChanged()
        {
            var propertiesChanged = new List<string>();

            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            detector.OnNext(new Image<Gray, byte>(1,1));

            Assert.Contains(propertiesChanged, s => nameof(IBackgroundSubtractionBasedPeopleDetector.Mask).Equals(s));
        }

        [Fact]
        public void SetMaskToNullWhenStreamEnds()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullWhenStreamErrorsOut()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullOnReset()
        {
            var detector = new CNTBackgroundSubtractionBasedPeopleDetector(decisionMaker.Object, logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.Mask);
        }
    }
}