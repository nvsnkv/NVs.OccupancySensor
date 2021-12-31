using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
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
        
        [Fact]
        //TODO: redesign blinking test
        public async Task UseProvidedSettingsToSetFrameInterval()
        {
            var settings = new CaptureSettings("Some source", TimeSpan.FromMilliseconds(50));

            var captureMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
            captureMock.Setup(c => c.QueryFrame()).Returns(() => new Image<Gray, byte>(100,100).Mat);

            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, _ => captureMock.Object);
            var observer = new TestImageObserver<Gray>();

            camera.Settings = settings;

            camera.Start();
            using (camera.Stream.Subscribe(observer))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                camera.Stop();
            }
            
            Assert.True(11 >= observer.ReceivedItems.Count);
            Assert.True(9 <= observer.ReceivedItems.Count);
        }

        [Fact]
        public async Task StopAutomaticallyIfVideoStreamFails()
        {
            var captureMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
            captureMock.Setup(c => c.QueryFrame()).Throws<TestException>();

            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, _ => captureMock.Object);
            var observer = new TestImageObserver<Gray>();
            
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

        [Fact]
        public void NotifyWhenItWasStarted()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged;

            camera.Start();

            Assert.True(camera.IsRunning);
            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.IsRunning)));
        }

        [Fact]
        public void ProvideNewStreamWhenStarted()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged;

            camera.Start();

            Assert.NotNull(camera.Stream);
            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }

        [Fact]
        public void NotifyWhenItWasStopped()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();

            camera.PropertyChanged += logger.OnPropertyChanged;

            camera.Start();
            camera.Stop();

            Assert.False(camera.IsRunning);
            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.IsRunning)));
        }

        [Fact]
        public void CompleteStreamWhenStopped()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();
            var observer = new TestImageObserver<Gray>();

            camera.PropertyChanged += logger.OnPropertyChanged;

            camera.Start();
            using (camera.Stream.Subscribe(observer))
            {
                camera.Stop();
            }

            Assert.Null(camera.Stream);
            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
            
            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public void PreventParallelAttemptsToStart()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();
            camera.PropertyChanged += logger.OnPropertyChanged;
            
            Task.WaitAll(Enumerable.Repeat(Task.Run(() => camera.Start()), Environment.ProcessorCount).ToArray());

            Assert.Equal(1, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }

        [Fact]
        public void PreventParallelAttemptsToStop()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, Camera.CreateVideoCapture);
            var logger = new PropertyChangedLogger();
            camera.PropertyChanged += logger.OnPropertyChanged;

            camera.Start();
            Task.WaitAll(Enumerable.Repeat(Task.Run(() => camera.Stop()), Environment.ProcessorCount).ToArray());

            Assert.Equal(2, logger.Notifications[camera].Count(x => x.Value == nameof(Camera.Stream)));
        }

        [Fact]
        public void RemainStoppedIfAttemptCreateNewVideoCaptureFailed()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, _ => throw new TestException());
            var logger = new PropertyChangedLogger();
            camera.PropertyChanged += logger.OnPropertyChanged;

            Assert.Throws<TestException>(() => camera.Start());
            Assert.False(camera.IsRunning);
            Assert.Empty(logger.Notifications);
        }

        [Fact]
        public void RethrowTheExceptionOccurredDuringVideoCaptureCreation()
        {
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, _ => throw new TestException());

            Assert.Throws<TestException>(() => camera.Start());
        }

        [Fact]
        public void LogTheExceptionOccurredDuringVideoCaptureCreation()
        {
            cameraLogger
                .Setup(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>>(),
                        It.IsAny<TestException>(),
                        It.IsAny<Func<It.IsSubtype<IReadOnlyList<KeyValuePair<string, object>>>, Exception, string>>()))
                .Verifiable("Logger was not called!");
            
            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, _ => throw new TestException());

            Assert.Throws<TestException>(() => camera.Start());
            cameraLogger.Verify();
        }

        [Fact]
        public void UseProvidedSettingsToCreateNewCapture()
        {
            CaptureSettings capturedSettings = null;
            CaptureSettings expectedSettings = new CaptureSettings("Some source", TimeSpan.Zero);

            var captureMock = new Mock<VideoCapture>(MockBehavior.Default, 0, VideoCapture.API.Any);
            captureMock.Setup(c => c.QueryFrame()).Returns(new Mat());

            var camera = new Camera(cameraLogger.Object, streamLogger.Object, CaptureSettings.Default, s =>
            {
                capturedSettings = s;
                return captureMock.Object;
            });

            camera.Settings = expectedSettings;
            
            camera.Start();
            Assert.Equal(expectedSettings, capturedSettings);
        }
    }
}