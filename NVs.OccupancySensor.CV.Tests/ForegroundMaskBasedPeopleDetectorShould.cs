using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class ForegroundMaskBasedPeopleDetectorShould
    {
        private readonly Mock<ILogger<ForegroundMaskBasedPeopleDetector>> logger = new Mock<ILogger<ForegroundMaskBasedPeopleDetector>>(); 

        [Fact]
        public void DetectPrecenceIfMaskIsWhite()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            };

            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.OnNext(image);

            Assert.True(detector.PeopleDetected);
        }

        [Fact]
        public void NotDetectPresenceIfMaskIsBlack()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(0);
            };

            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.OnNext(image);

            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void NotifyWhenPeopleDetected()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            };

            var propertyName = string.Empty;
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.PropertyChanged += (_, e) => propertyName = e.PropertyName;
            
            detector.OnNext(image);

            Assert.Equal(nameof(IPeopleDetector.PeopleDetected), propertyName);
        }

        [Fact]
        public void NotifyWhenPeopleNotDetected()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(0);
            };

            var propertyName = string.Empty;
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.PropertyChanged += (_, e) => propertyName = e.PropertyName;
            
            detector.OnNext(image);

            Assert.Equal(nameof(IPeopleDetector.PeopleDetected), propertyName);
        }

        [Fact]
        public void NotifyWhenDetectionIsNotPossible()
        {
            var propertyName = string.Empty;
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.PropertyChanged += (_, e) => propertyName = e.PropertyName;
            
            detector.OnNext(new Image<Gray, byte>(1,1));

            Assert.Equal(nameof(IPeopleDetector.PeopleDetected), propertyName);
        }
        
        [Fact]
        public void SetPeopleDetectedToNullWhenStreamEnds()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullWhenStreamErrorsOut()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullOnReset()
        {
            var detector = new ForegroundMaskBasedPeopleDetector(logger.Object, DetectionSettings.Default.Threshold);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.PeopleDetected);
        }
    }
}