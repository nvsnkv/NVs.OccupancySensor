using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.BackgroundSubtraction.DecisionMaking;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Detection.Correction;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{

    public sealed class BackgroundSubtractionBasedDetectorShould
    {
        private readonly Mock<IDecisionMaker> decisionMaker = new Mock<IDecisionMaker>();
        private readonly Mock<ILogger<BackgroundSubtractionBasedDetector>> logger = new Mock<ILogger<BackgroundSubtractionBasedDetector>>();
        private readonly Mock<ISubtractionStrategy> subtractor = new Mock<ISubtractionStrategy>();
        private readonly Mock<IBackgroundSubtractorFactory> factory = new Mock<IBackgroundSubtractorFactory>();
        private readonly Mock<ICorrectionStrategyFactory> correctionFactory = new Mock<ICorrectionStrategyFactory>();
        private readonly Mock<ICorrectionStrategy> corrector = new Mock<ICorrectionStrategy>();

        public BackgroundSubtractionBasedDetectorShould()
        {
            subtractor
                .Setup(s => s.GetForegroundMask(It.IsAny<Image<Rgb, byte>>()))
                .Returns(new Image<Gray, byte>(1, 1));

            corrector
                .Setup(s => s.Apply(It.IsAny<Image<Gray, byte>>()))
                .Returns(new Image<Gray, byte>(1, 1));

            factory.Setup(f => f.Create(SupportedAlgorithms.CNT.ToString())).Returns(subtractor.Object);
            correctionFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(corrector.Object);
        }

        [Fact]
        public void NotifyWhenPeopleDetected()
        {
            decisionMaker.Setup(m => m.DetectPresence(It.IsAny<Image<Gray, byte>>())).Returns(true);

            var propertiesChanged = new List<string>();
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Rgb, byte>(1, 1));

            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }

        [Fact]
        public void NotifyWhenPeopleNotDetected()
        {
            decisionMaker.Setup(m => m.DetectPresence(It.IsAny<Image<Gray, byte>>())).Returns(false);

            var propertiesChanged = new List<string>();
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);
            
            detector.OnNext(new Image<Rgb, byte>(1, 1));
            Assert.Contains(propertiesChanged, s => nameof(IPeopleDetector.PeopleDetected).Equals(s));
        }
     
        [Fact]
        public void SetPeopleDetectedToNullWhenStreamEnds()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullWhenStreamErrorsOut()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullOnReset()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.PeopleDetected);
        }


        [Fact]
        public void RaisePropertyChangedWhenMaskChanged()
        {
            var propertiesChanged = new List<string>();

            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            detector.OnNext(new Image<Rgb, byte>(1,1));

            Assert.Contains(propertiesChanged, s => nameof(IBackgroundSubtractionBasedDetector.Mask).Equals(s));
        }

        [Fact]
        public void SetMaskToNullWhenStreamEnds()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.OnCompleted();
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullWhenStreamErrorsOut()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.OnError(new System.Exception());
            Assert.Null(detector.Mask);
        }

        [Fact]
        public void SetMaskToNullOnReset()
        {
            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.OnNext(new Image<Rgb, byte>(1,1));

            detector.Reset();
            Assert.Null(detector.Mask);
        }

        [Fact]
        public async Task DropNewFramesIfSubtractorIsStillCalculating()
        {
            var maskChanged = 0;
            subtractor
                .Setup(s => s.GetForegroundMask(It.IsAny<Image<Rgb, byte>>()))
                .Returns(() =>
                {
                    Task.Delay(TimeSpan.FromMilliseconds(600)).Wait();
                    return new Image<Gray, byte>(1, 1);
                })
                .Verifiable();

            var detector = new BackgroundSubtractionBasedDetector(factory.Object, correctionFactory.Object, decisionMaker.Object ,logger.Object, DetectionSettings.Default);
            detector.PropertyChanged += (_, e) => maskChanged += nameof(detector.Mask).Equals(e.PropertyName) ? 1 : 0;

            Enumerable.Range(0, 3).ToList().ForEach(_ => Task.Factory.StartNew(() => detector.OnNext(new Image<Rgb, byte>(1, 1))));
            
            await Task.Delay(TimeSpan.FromMilliseconds(2000));
            Assert.Equal(1, maskChanged);
        }
    }
}