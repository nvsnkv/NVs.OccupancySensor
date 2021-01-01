using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class OccupancySensorShould
    {
        private readonly Mock<ICamera> camera = new Mock<ICamera>();
        private readonly Mock<IMatConverter> converter = new Mock<IMatConverter>();
        private readonly Mock<IPeopleDetector> detector = new Mock<IPeopleDetector>();

        private readonly Mock<ILogger<Impl.OccupancySensor>> logger = new Mock<ILogger<Impl.OccupancySensor>>(); 

        [Fact]
        public void StartCamearOnStart()
        {
            camera.Setup(c => c.Start()).Verifiable("Start was not requested");
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);

            sensor.Start();
            camera.Verify();
        }

        [Fact]
        public void StopCamearOnStop()
        {
            camera.Setup(c => c.Stop()).Verifiable("Stop was not requested");
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);

            sensor.Stop();
            camera.Verify();
        }

        [Fact]
        public void RaiseIsRunningPropertyChangedWhenCameraRaisesCorrespondingPropertyChanged() 
        {
            var propertyName = string.Empty;
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));
            Assert.Equal(nameof(Impl.OccupancySensor.IsRunning), propertyName);
        }

        [Fact]
        public void CallDetectorResetWhenCameraStopped() 
        {
            camera.SetupGet(c => c.IsRunning).Returns(false);
            detector.Setup(d => d.Reset()).Verifiable("Reset was not called!");
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            
            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));
            detector.Verify();
        }

        [Fact]
        public async Task SetupStreamWhenCameraStarted() 
        {
            var capture = new Mock<VideoCapture>(MockBehavior.Loose, 0, VideoCapture.API.Any);
            capture.Setup(c => c.QueryFrame()).Returns(() => new Mat());

            camera.SetupGet(c => c.IsRunning).Returns(true);
            camera.SetupGet(c => c.Stream).Returns(new CameraStream(capture.Object, CancellationToken.None, new Mock<ILogger<CameraStream>>().Object, TimeSpan.FromMilliseconds(100)));
            
            converter.Setup(c => c.Convert(It.IsAny<Mat>())).Returns(() => new Image<Rgb, int>(100,100)).Verifiable("Convert was not called!");
            detector.Setup(d => d.Detect(It.IsAny<Image<Rgb,int>>())).Returns<Image<Rgb,int>>(x => x).Verifiable("Detect was not called!");
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            
            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));
            Assert.NotNull(sensor.Stream);


            await Task.Delay(TimeSpan.FromMilliseconds(200));
            converter.Verify();
            detector.Verify();
        }

        [Fact]
        public void RaisePresenceDetectedProperryChangedWhenDetectorRaisesCorrespondingPropertyChanged()
        {
            var propertyName = string.Empty;
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            detector.Raise(d => d.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPeopleDetector.PeopleDetected)));
            Assert.Equal(nameof(Impl.OccupancySensor.PresenceDetected), propertyName);           
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnSameValueAsPeopleDetectorFromPresenceDetected(bool? expected)
        {
            detector.SetupGet(d => d.PeopleDetected).Returns(expected);
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);

            Assert.Equal(expected, sensor.PresenceDetected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnSameValueAsCameraFromIsRunning(bool expected)
        {
            camera.SetupGet(c => c.IsRunning).Returns(expected);
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);

            Assert.Equal(expected, sensor.IsRunning);
        }

        [Fact]
        public void UnsubscribeItselfFromCameraAfterDispose() 
        {
            bool propertyChangedRaised = false;
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_,__) => propertyChangedRaised = true;

            sensor.Dispose();
            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));

            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void UnsubscribeItselfFromDetectorAfterDispose() 
        {
            bool propertyChangedRaised = false;
            var sensor = new Impl.OccupancySensor(camera.Object, converter.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_,__) => propertyChangedRaised = true;

            sensor.Dispose();
            detector.Raise(d => d.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPeopleDetector.PeopleDetected)));

            Assert.False(propertyChangedRaised);
        }
    }
}