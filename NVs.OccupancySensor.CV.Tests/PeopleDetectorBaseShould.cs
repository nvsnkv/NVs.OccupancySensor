using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl.Detectors;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class PeopleDetectorShould
    {
        private static readonly Image<Rgb,byte> TestImage = new Image<Rgb,byte>(100, 100);
        private readonly Mock<ILogger<TestPeopleDetector>> logger = new Mock<ILogger<TestPeopleDetector>>();

        [Fact]
        public void ThrowErrorsHappenedDuringDetection()
        {
            var detector = new TestPeopleDetector(logger.Object, _ => throw new TestException(), _ => {});

            Assert.Throws<TestException>(() => detector.Detect(TestImage));
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
            
            var detector = new TestPeopleDetector(logger.Object, _ => throw new TestException(), _ => {});

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

            var invokedCount = 0;
            
            var detector = new TestPeopleDetector(logger.Object, _ =>
            {
                invokedCount++;
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                return new Rectangle[] { };
            }, _ => { });

            Task.WaitAll(new Task[] { Task.Run(() => detector.Detect(TestImage)), Task.Run(() => detector.Detect(TestImage)) });
            
            Assert.Equal(1, invokedCount);
        }

        [Fact]
        public void SetPeopleDetectedToTrueIfDetectionReturnedSomeAreas()
        {
            var detector = new TestPeopleDetector(logger.Object, _ => new[] { new Rectangle() }, _ => { });

            detector.Detect(TestImage);
            Assert.True(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToFalseIfDetectionReturnedEmptyResult()
        {
            var detector = new TestPeopleDetector(logger.Object, _ => new Rectangle[] { }, _ => { });

            detector.Detect(TestImage);
            Assert.False(detector.PeopleDetected);
        }

        [Fact]
        public void SetPeopleDetectedToNullIfDetectionFailed()
        {
            var detector = new TestPeopleDetector(logger.Object, _ => throw new TestException(), _ => { });

            try { detector.Detect(TestImage); } catch { }
            
            Assert.Null(detector.PeopleDetected);
        }

        [Fact]
        public void NotifyPeopleDetectedChangedWhenDetectionReturnedSomeAreas()
        {
            string changedPropertyName = null;

            var detector = new TestPeopleDetector(logger.Object, _ => new[] { new Rectangle() }, _ => { });
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(PeopleDetectorBase.PeopleDetected), changedPropertyName);
        }

        [Fact]
        public void NotifyPeopleDetectedChangedWhenDetectionReturnedEmptyResult()
        {
            string changedPropertyName = null;
            var detector = new TestPeopleDetector(logger.Object, _ => new Rectangle[] { }, _ => { });
            detector.PropertyChanged += (_, e) => changedPropertyName = e.PropertyName;

            detector.Detect(TestImage);
            Assert.Equal(nameof(PeopleDetectorBase.PeopleDetected), changedPropertyName);
        }

        [Fact]
        public void SetPeopleDetectedToNullAfterReset()
        {
            var detector = new TestPeopleDetector(logger.Object, _ => new[] { new Rectangle() }, _ => { });

            detector.Detect(TestImage);
            detector.Reset();

            Assert.Null(detector.PeopleDetected);
        }
    }
}