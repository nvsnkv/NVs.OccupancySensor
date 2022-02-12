using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;


namespace NVs.OccupancySensor.CV.Sense
{
    internal sealed class OccupancySensor : IOccupancySensor, IDisposable
    {
        private readonly ICamera camera;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;

        private readonly IDisposable denoiserSubscription;
        private readonly IDisposable subtractorSubscription;
        private readonly IDisposable correctorSubscription;
        private readonly IDisposable detectorSubscription;
        private bool isDisposed;

        public OccupancySensor(ICamera camera, IDenoiser denoiser, IBackgroundSubtractor subtractor, ICorrector corrector, IPeopleDetector detector, ILogger<OccupancySensor> logger)
        {
            this.camera = camera;
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.detector = detector;
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger;

            subtractorSubscription = camera.Stream.Subscribe(subtractor);
            denoiserSubscription = subtractor.Output.Subscribe(denoiser);
            correctorSubscription = denoiser.Output.Subscribe(corrector);
            detectorSubscription = corrector.Output.Subscribe(detector);
        }

        public bool? PresenceDetected => detector.PeopleDetected;

        public bool IsRunning => camera.IsRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

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
            if (!isDisposed)
            {

                detectorSubscription.Dispose();
                correctorSubscription.Dispose();
                denoiserSubscription.Dispose();
                subtractorSubscription.Dispose();

                detector.PropertyChanged -= OnDetectorPropertyChanged;
                camera.PropertyChanged -= OnCameraPropertyChanged;

                isDisposed = true;
            }
        }

        private void OnCameraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ICamera.IsRunning):
                    logger.LogInformation($"Camera.IsRunning changed to {camera.IsRunning}");

                    if (!camera.IsRunning)
                    {
                        detector.Reset();
                    }

                    OnPropertyChanged(nameof(IsRunning));
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
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}