using System;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    [Collection("Run Exclusively")]
    public sealed class CameraShouldExclusively
    {
        private readonly Mock<ILogger<Camera>> cameraLogger = new();
        private readonly Mock<ILogger<CameraStream>> streamLogger = new(); 
        private readonly Mock<VideoCapture> captureMock = new(MockBehavior.Default, 0, VideoCapture.API.Any);

        [Fact]
        public async Task StopAutomaticallyIfVideoStreamFails()
        {
            captureMock.Setup(c => c.QueryFrame()).Throws<TestException>();

            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var observer = new TestImageObserver();
            
            camera.Start();
            using (camera.Stream.Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            Assert.IsType<TestException>(observer.Error);
            Assert.False(camera.IsRunning);
        }


    }

    public sealed class CameraShould
    {
        private readonly Mock<ILogger<Camera>> cameraLogger = new();
        private readonly Mock<ILogger<CameraStream>> streamLogger = new();
        private readonly Mock<VideoCapture> captureMock = new(MockBehavior.Default, 0, VideoCapture.API.Any);

        [Fact]
        public void NotifyWhenItWasStarted()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged!;

            camera.Start();

            Assert.True(camera.IsRunning);
            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.IsRunning)));
        }

        [Fact]
        public void ProvideNewStreamWhenStarted()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged!;

            camera.Start();

            Assert.NotNull(camera.Stream);
            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }

        [Fact]
        public void NotifyWhenItWasStopped()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged!;

            camera.Start();
            camera.Stop();

            Assert.False(camera.IsRunning);
            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.IsRunning)));
        }

        [Fact]
        public void NotCompleteStreamWhenStopped()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();
            var observer = new TestImageObserver();

            camera.PropertyChanged += logger.OnPropertyChanged!;

            camera.Start();
            using (camera.Stream.Subscribe(observer))
            {
                camera.Stop();
            }

            Assert.NotNull(camera.Stream);
            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
            
            Assert.False(observer.StreamCompleted);
        }

        [Fact]
        public void PreventParallelAttemptsToStart()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();
            camera.PropertyChanged += logger.OnPropertyChanged!;
            
            Task.WaitAll(Enumerable.Repeat(Task.Run(() => camera.Start()), Environment.ProcessorCount).ToArray());

            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }

        [Fact]
        public void PreventParallelAttemptsToStop()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, () => captureMock.Object);
            var logger = new PropertyChangedLogger();
            camera.PropertyChanged += logger.OnPropertyChanged!;

            camera.Start();
            Task.WaitAll(Enumerable.Repeat(Task.Run(() => camera.Stop()), Environment.ProcessorCount).ToArray());

            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }
    }
}