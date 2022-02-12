using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Capture
{
    internal sealed class Camera : ICamera
    {
        private readonly object thisLock = new object();

        private readonly ILogger<Camera> logger;
        private readonly ILogger<CameraStream> streamLogger;
        private readonly Func<VideoCapture> createVideoCaptureFunc;
        private readonly ErrorObserver errorObserver;

        private VideoCapture capture;
        private CancellationTokenSource cts;

        private ICameraStream stream;
        private volatile bool isRunning;

        public Camera(ILogger<Camera> logger, ILogger<CameraStream> streamLogger, CaptureSettings settings, Func<VideoCapture> createVideoCaptureFunc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.streamLogger = streamLogger ?? throw new ArgumentNullException(nameof(streamLogger));
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.createVideoCaptureFunc = createVideoCaptureFunc ?? throw new ArgumentNullException(nameof(createVideoCaptureFunc));
            this.errorObserver = new ErrorObserver(this);

            SetupStream();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IObservable<Image<Gray, byte>> Stream => stream;

        public bool IsRunning => isRunning;

        public CaptureSettings Settings { get; }

        public void Start()
        {
            logger.LogInformation("Attempting to start camera...");
            SetIsRunning();

            try
            {
                stream.Resume();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while setting up new stream!");
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

            stream.Pause();

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
        
        private void SetupStream()
        {
            logger.LogInformation("Setting up new stream...");
            cts = new CancellationTokenSource();

            capture = createVideoCaptureFunc();
            
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

        private class ErrorObserver : IObserver<Image<Gray, byte>>
        {
            private readonly Camera camera;

            public ErrorObserver(Camera camera)
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

            public void OnNext(Image<Gray, byte> value)
            {
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}