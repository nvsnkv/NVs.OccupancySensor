using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;
using Xunit;
using IBackgroundSubtractor = NVs.OccupancySensor.CV.BackgroundSubtraction.IBackgroundSubtractor;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class OccupancySensorShould
    {
        private readonly Mock<ICamera> camera = new();
        private readonly Mock<IDenoiser> denoiser = new();
        private readonly Mock<IBackgroundSubtractor> subtractor = new();
        private readonly Mock<ICorrector> corrector = new();
        private readonly Mock<IPeopleDetector> detector = new();
        private readonly Mock<ILogger<Sense.OccupancySensor>> logger = new();

        public OccupancySensorShould()
        {
            camera.SetupGet(c => c.Stream).Returns(GetStreamMock());
            denoiser.SetupGet(d => d.Output).Returns(GetStreamMock());
            subtractor.SetupGet(d => d.Output).Returns(GetStreamMock());
            corrector.SetupGet(d => d.Output).Returns(GetStreamMock());
        }

        private static IObservable<Image<Gray, byte>> GetStreamMock()
        {
            var mock = new Mock<IObservable<Image<Gray, byte>>>();
            mock.Setup(s => s.Subscribe(It.IsAny<IObserver<Image<Gray, byte>>>())).Returns(new Mock<IDisposable>().Object);

            return mock.Object;
        }

        [Fact]
        public void StartCameraOnStart()
        {
            camera.Setup(c => c.Start()).Verifiable("Start was not requested");
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            sensor.Start();
            camera.Verify();
        }

        [Fact]
        public void StopCameraOnStop()
        {
            camera.Setup(c => c.Stop()).Verifiable("Stop was not requested");
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            sensor.Stop();
            camera.Verify();
        }

        [Fact]
        public void RaiseIsRunningPropertyChangedWhenCameraRaisesCorrespondingPropertyChanged() 
        {
            var propertyName = string.Empty;
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));
            Assert.Equal(nameof(Sense.OccupancySensor.IsRunning), propertyName);
        }

        [Fact]
        public void CallDetectorResetWhenCameraStopped() 
        {
            detector.Setup(d => d.Reset()).Verifiable("Reset was not called!");
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            sensor.Stop();
            detector.Verify();
        }
        
        [Fact]
        public async Task ConnectStreamToDenoiser()
        {
            var capture = new Mock<VideoCapture>(MockBehavior.Loose, 0, VideoCapture.API.Any);
            capture.Setup(c => c.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);

            camera.SetupGet(c => c.IsRunning).Returns(true);
            var cameraStream = new CameraStream(capture.Object, CancellationToken.None, new Mock<ILogger<CameraStream>>().Object, TimeSpan.FromMilliseconds(100));
            cameraStream.Resume();

            camera.SetupGet(c => c.Stream).Returns(cameraStream);
            subtractor.SetupGet(d => d.Output).Returns(cameraStream);
            
            denoiser.Setup(d => d.OnNext(It.IsAny<Image<Gray, byte>>())).Verifiable("Denoise was not called!");
            var _ = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            await Task.Delay(TimeSpan.FromMilliseconds(200));
            denoiser.Verify();
        }

        [Fact]
        public async Task ConnectStreamToDetector()
        {
            var capture = new Mock<VideoCapture>(MockBehavior.Loose, 0, VideoCapture.API.Any);
            capture.Setup(c => c.QueryFrame()).Returns(() => new Image<Gray, byte>(100, 100).Mat);

            camera.SetupGet(c => c.IsRunning).Returns(true);
            camera.SetupGet(c => c.Stream).Returns(new CameraStream(capture.Object, CancellationToken.None, new Mock<ILogger<CameraStream>>().Object, TimeSpan.FromMilliseconds(100)));
            denoiser.SetupGet(d => d.Output).Returns(new CameraStream(capture.Object, CancellationToken.None, new Mock<ILogger<CameraStream>>().Object, TimeSpan.FromMilliseconds(100)));
            
            denoiser.Setup(d => d.OnNext(It.IsAny<Image<Gray, byte>>())).Verifiable("Detect was not called!");
            var _ = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            detector.Verify();
        }

        [Fact]
        public void RaisePresenceDetectedPropertyChangedWhenDetectorRaisesCorrespondingPropertyChanged()
        {
            var propertyName = string.Empty;
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            detector.Raise(d => d.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPeopleDetector.PeopleDetected)));
            Assert.Equal(nameof(Sense.OccupancySensor.PresenceDetected), propertyName);           
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnSameValueAsPeopleDetectorFromPresenceDetected(bool? expected)
        {
            detector.SetupGet(d => d.PeopleDetected).Returns(expected);
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            Assert.Equal(expected, sensor.PresenceDetected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReturnSameValueAsCameraFromIsRunning(bool expected)
        {
            camera.SetupGet(c => c.IsRunning).Returns(expected);
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            Assert.Equal(expected, sensor.IsRunning);
        }

        [Fact]
        public void UnsubscribeItselfFromCameraAfterDispose() 
        {
            bool propertyChangedRaised = false;
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_,_) => propertyChangedRaised = true;

            sensor.Dispose();
            camera.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ICamera.IsRunning)));

            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void UnsubscribeItselfFromDetectorAfterDispose() 
        {
            bool propertyChangedRaised = false;
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            sensor.PropertyChanged += (_,_) => propertyChangedRaised = true;

            sensor.Dispose();
            detector.Raise(d => d.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPeopleDetector.PeopleDetected)));

            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void ResetSubtractorOnStop()
        {
            subtractor.Setup(s => s.Reset()).Verifiable();
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);
            
            sensor.Stop();
            subtractor.Verify(s => s.Reset());
        }

        [Fact]
        public void ResetCorrectorOnStop()
        {
            corrector.Setup(s => s.Reset()).Verifiable();
            var sensor = new Sense.OccupancySensor(camera.Object, denoiser.Object, subtractor.Object, corrector.Object, detector.Object, logger.Object);

            sensor.Stop();
            corrector.Verify(s => s.Reset());
        }
    }
}