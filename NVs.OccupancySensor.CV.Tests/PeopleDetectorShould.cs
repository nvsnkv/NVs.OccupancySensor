using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    
    public sealed class PeopleDetectorShould
    {
        private readonly Mock<ILogger<PeopleDetector>> logger = new Mock<ILogger<PeopleDetector>>(); 

        [Fact]
        public void DetectPresenceIfMaskIsWhite()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            }

            var detector = new PeopleDetector(DetectionSettings.Default, logger.Object);
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
            }

            var detector = new PeopleDetector(DetectionSettings.Default, logger.Object);
            detector.OnNext(image);

            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void RespectSettingsChange()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            }

            var detector = new PeopleDetector(DetectionSettings.Default, logger.Object);
            detector.OnNext(image);

            Assert.True(detector.PeopleDetected);

            detector.Settings = new DetectionSettings(1.1);
            detector.Reset();

            detector.OnNext(image);

            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void NotifyWhenPeopleDetected()
        {
            var propertiesChanged = new List<string>();
            var detector = new PeopleDetector(new DetectionSettings(0), logger.Object);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Gray, byte>(1, 1));

            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }

        [Fact]
        public void NotifyWhenPeopleNotDetected()
        {
            var propertiesChanged = new List<string>();
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Gray, byte>(1, 1));
            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }
     
        [Fact]
        public void SetPeopleDetectedToNullWhenStreamEnds()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullWhenStreamErrorsOut()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullOnReset()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.PeopleDetected);
        }


        [Fact]
        public void RaisePropertyChangedWhenMaskChanged()
        {
            var propertiesChanged = new List<string>();

            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            detector.OnNext(new Image<Gray, byte>(1,1));

            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.Mask).Equals(s));
        }

        [Fact]
        public void SetMaskToNullWhenStreamEnds()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullWhenStreamErrorsOut()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullOnReset()
        {
            var detector = new PeopleDetector(new DetectionSettings(1), logger.Object);
            detector.OnNext(new Image<Gray, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.Mask);
        }
    }
}