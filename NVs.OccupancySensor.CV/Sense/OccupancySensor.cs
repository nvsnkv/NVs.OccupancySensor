using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;


namespace NVs.OccupancySensor.CV.Sense
{
    internal sealed class OccupancySensor : IOccupancySensor, IDisposable
    {
        private readonly ICamera camera;
        private readonly IDenoiser denoiser;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;
        
        private IDisposable cameraSubscription;
        private IDisposable denoiserSubscription;
        private bool isDisposed;

        public OccupancySensor([NotNull] ICamera camera, [NotNull] IDenoiser denoiser, [NotNull] IPeopleDetector detector, [NotNull] ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.denoiser = denoiser ?? throw new ArgumentNullException(nameof(denoiser));
        }

        public bool? PresenceDetected => detector.PeopleDetected;

        public bool IsRunning => camera.IsRunning;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Start()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(OccupancySensor));
            }

            logger.LogInformation("Start requested");
            camera.Start();
        }

        public void Stop()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(OccupancySensor));
            }

            logger.LogInformation("Stop requested");
            camera.Stop();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        private void OnCameraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ICamera.IsRunning):
                    logger.LogInformation($"Camera.IsRunning changed to {camera.IsRunning}");
                    OnPropertyChanged(nameof(IsRunning));
                    if (camera.IsRunning)
                    {
                        cameraSubscription = camera.Stream.Subscribe(denoiser);
                        denoiserSubscription = denoiser.Output.Subscribe(detector);
                    }
                    else
                    {
                        cameraSubscription?.Dispose();
                        denoiserSubscription?.Dispose();

                        detector.Reset();
                        denoiser.Reset();
                    }
                    break;
            }
        }

        private void OnDetectorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPeopleDetector.PeopleDetected):
                    logger.LogInformation($"Detector.PeopleDetected changed to {detector.PeopleDetected?.ToString() ?? "null"}");
                    OnPropertyChanged(nameof(PresenceDetected));
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    cameraSubscription?.Dispose();
                    denoiserSubscription?.Dispose();

                    camera.PropertyChanged -= OnCameraPropertyChanged;
                    detector.PropertyChanged -= OnDetectorPropertyChanged;
                }

                isDisposed = true;
            }
        }
    }
}