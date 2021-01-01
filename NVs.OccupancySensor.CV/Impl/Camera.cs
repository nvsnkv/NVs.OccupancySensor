using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Emgu.CV;
using Microsoft.Extensions.Logging;
using JetBrains.Annotations;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class Camera : ICamera
    {
        private readonly object thisLock = new object();

        private readonly ILogger<Camera> logger;
        private readonly ILogger<CameraStream> streamLogger;
        private readonly Func<CameraSettings, VideoCapture> createVideoCaptureFunc;
        private readonly ErrorObserver errorObserver;

        private VideoCapture capture;
        private CancellationTokenSource cts;

        private ICameraStream stream;
        private volatile bool isRunning;
        private CameraSettings settings;

        public Camera([NotNull] ILogger<Camera> logger, [NotNull] ILogger<CameraStream> streamLogger, [NotNull] CameraSettings settings, [NotNull] Func<CameraSettings, VideoCapture> createVideoCaptureFunc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.streamLogger = streamLogger ?? throw new ArgumentNullException(nameof(streamLogger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.createVideoCaptureFunc = createVideoCaptureFunc ?? throw new ArgumentNullException(nameof(createVideoCaptureFunc));
            this.errorObserver = new ErrorObserver(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICameraStream Stream => stream;

        public bool IsRunning => isRunning;

        public CameraSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value, settings)) return;
                settings = value;
                OnPropertyChanged();
            }
        }

        public void Start()
        {
            logger.LogInformation("Attempting to start camera...");
            SetIsRunning();

            try
            {
                SetupStream();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while setting up new stream!");
                CompleteStream();
                isRunning = false;

                throw;
            }

            logger.LogInformation("Camera is now running.");

            OnPropertyChanged(nameof(Stream));
            OnPropertyChanged(nameof(IsRunning));
            logger.LogInformation("Change notification complete");
        }

        public void Stop()
        {
            logger.LogInformation("Attempting to stop camera...");
            UnsetIsRunning();

            CompleteStream();

            logger.LogInformation("Camera is now stopped");

            OnPropertyChanged(nameof(Stream));
            OnPropertyChanged(nameof(IsRunning));
            logger.LogInformation("Change notification complete");
        }

        private void UnsetIsRunning()
        {
            if (!isRunning)
            {
                logger.LogWarning("The camera is already stopped, no action will be taken.");
            }

            lock (thisLock)
            {
                if (!isRunning)
                {
                    logger.LogWarning("The camera is already stopped, no action will be taken.");
                }

                isRunning = false;
            }
        }

        private void CompleteStream()
        {
            logger.LogInformation("Finalizing current stream and releasing resources...");
            try
            {
                cts?.Cancel();
                capture?.Dispose();

                stream = null;
                capture = null;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to finalize current stream!");
                throw;
            }

            logger.LogInformation("Stream finalization is completed.");
        }

        private void SetupStream()
        {
            logger.LogInformation("Setting up new stream...");
            cts = new CancellationTokenSource();

            capture = createVideoCaptureFunc(Settings);

            stream = new CameraStream(capture, cts.Token, streamLogger, Settings.FrameInterval);
            stream.Subscribe(errorObserver);
            logger.LogInformation("Stream has been created");
        }

        private void SetIsRunning()
        {
            if (isRunning)
            {
                logger.LogWarning("The camera is already running, no action will be taken.");
            }

            lock (thisLock)
            {
                if (isRunning)
                {
                    logger.LogWarning("The camera is already running, no action will be taken.");
                }

                isRunning = true;
            }
        }

        private class ErrorObserver : IObserver<Mat>
        {
            private readonly Camera camera;

            public ErrorObserver([NotNull] Camera camera)
            {
                this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
                camera.logger.LogError(error, "Received an error from CameraStream. Camera will be stopped");
                camera.Stop();
            }

            public void OnNext(Mat value)
            {
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static VideoCapture CreateVideoCapture(CameraSettings settings)
        {
            return int.TryParse(settings.Source, out var camIndex)
                ? new VideoCapture(camIndex)
                : new VideoCapture(settings.Source);
        }
    }
}