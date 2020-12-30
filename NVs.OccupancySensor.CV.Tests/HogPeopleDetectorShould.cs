using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
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
            var descriptorMock = new Mock<HOGDescriptor>();
            descriptorMock.Setup(d => d.Dispose()).Verifiable("Dispose was not called!");
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptorMock.Object);
            
            detector.Dispose();
            descriptorMock.Verify();
        }

        [Fact]
        public void ThrowErrorsHappenedDuringDetection()
        {
            var descriptorMock = new Mock<HOGDescriptor>();
            descriptorMock
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
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
            
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Throws<TestException>();

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            try {  detector.Detect(TestImage); } catch { }

            logger.Verify();
        }

        [Fact]
        public void PreventsParallelDetection()
        {
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Returns(() =>
                {
                    Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                    return new MCvObjectDetection[] {};
                })
                .Verifiable();

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            detector.Detect(TestImage);
            detector.Detect(TestImage);
            
            descriptor.Verify(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false), Times.Once);
        }

        [Fact]
        public void SetPeopleDetectedToTrueIfDetectionReturnedSomeAreas()
        {
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Returns(() => new[] {new MCvObjectDetection()});

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            detector.Detect(TestImage);
            Assert.True(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToFalseIfDetectionReturnedEmptyResult()
        {
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Returns(() => new MCvObjectDetection[] { });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            detector.Detect(TestImage);
            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullIfDetectionFailed()
        {
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Throws<TestException>();
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);

            try{ detector.Detect(TestImage); } catch { }
            
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void NotifyPeopleDetectedChangedWhenDetectionReturnedSomeAreas()
        {
            string changedPropertyName = null;
            
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Returns(() => new[] { new MCvObjectDetection() });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(HogPeopleDetector.PeopleDetected), changedPropertyName);
        }

        [Fact]
        public void SNotifyPeopleDetectedChangedWhenDetectionReturnedEmptyResult()
        {
            string changedPropertyName = null;
            var descriptor = new Mock<HOGDescriptor>();
            descriptor
                .Setup(d => d.DetectMultiScale(It.IsAny<Image<Rgb, int>>(), 0D, default, default, 1.05D, 2D, false))
                .Returns(() => new MCvObjectDetection[] { });

            var detector = new HogPeopleDetector(logger.Object, () => descriptor.Object);
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(HogPeopleDetector.PeopleDetected), changedPropertyName);
        }
    }
}