using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl.HOG;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class HogPeopleDetectorShould
    {
        private static readonly Image<Rgb, int> TestImage = new Image<Rgb, int>(100, 100);
        
        private readonly Mock<ILogger<HogPeopleDetector>> logger;

        public HogPeopleDetectorShould()
        {
            this.logger = new Mock<ILogger<HogPeopleDetector>>(MockBehavior.Loose);
        }

        [Fact]
        public void DisposeDescriptorWhenDisposed()
        {
            var descriptorMock = new Mock<IHOGDescriptorWrapper>();
            descriptorMock.Setup(d => d.Dispose()).Verifiable("Dispose was not called!");
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptorMock.Object);
            
            detector.Dispose();
            descriptorMock.Verify();
        }

        [Fact]
        public void ThrowErrorsHappenedDuringDetection()
        {
            var descriptorMock = new Mock<IHOGDescriptorWrapper>();
            descriptorMock
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Throws<TestException>();

            var image = new Image<Rgb, int>(100, 100);
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptorMock.Object);

            Assert.Throws<TestException>(() => detector.Detect(image));
        }

        [Fact]
        public void LogErrorsHappenedDuringDetection()
        {
            logger.Setup(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                        It.IsAny<TestException>(),
                        It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");
            
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Throws<TestException>();

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            try {  detector.Detect(TestImage); } catch { }

            logger.Verify();
        }

        [Fact]
        public void PreventsParallelDetection()
        {
            if (Environment.ProcessorCount == 1) 
            {
                return;
            }

            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() =>
                {
                    Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                    return new MCvObjectDetection[] {};
                })
                .Verifiable();

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            Task.WaitAll(new Task[] { Task.Run(() => detector.Detect(TestImage)), Task.Run(() => detector.Detect(TestImage)) });
            
            descriptor.Verify(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()), Times.Once);
        }

        [Fact]
        public void SetPeopleDetectedToTrueIfDetectionReturnedSomeAreas()
        {
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() => new[] {new MCvObjectDetection()});

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            detector.Detect(TestImage);
            Assert.True(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToFalseIfDetectionReturnedEmptyResult()
        {
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() => new MCvObjectDetection[] { });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            detector.Detect(TestImage);
            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullIfDetectionFailed()
        {
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Throws<TestException>();
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            try{ detector.Detect(TestImage); } catch { }
            
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void NotifyPeopleDetectedChangedWhenDetectionReturnedSomeAreas()
        {
            string changedPropertyName = null;
            
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() => new[] { new MCvObjectDetection() });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(HogPeopleDetector.PeopleDetected), changedPropertyName);
        }

        [Fact]
        public void NotifyPeopleDetectedChangedWhenDetectionReturnedEmptyResult()
        {
            string changedPropertyName = null;
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() => new MCvObjectDetection[] { });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(HogPeopleDetector.PeopleDetected), changedPropertyName);
        }

        [Fact]
        public void SetPeopleDetectedToNullAfterReset()
        {
            var descriptor = new Mock<IHOGDescriptorWrapper>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>()))
                .Returns(() => new MCvObjectDetection[] { });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);
            
            detector.Detect(TestImage);
            detector.Reset();

            Assert.Null(detector.PeopleDetected);
        }
    }
}