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
        private readonly IDenoiser denoiser;
        [NotNull] private readonly IBackgroundSubtractor subtractor;
        private readonly ICorrector corrector;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;
        
        private IDisposable denoiserSubscription;
        private IDisposable subtractorSubscription;
        private IDisposable correctorSubscription;
        private IDisposable detectorSubscription;
        private bool isDisposed;
        private bool? presenceDetected;
        private bool isRunning;

        public OccupancySensor([NotNull] ICamera camera, [NotNull] IDenoiser denoiser, [NotNull] IBackgroundSubtractor subtractor,
            [NotNull] ICorrector corrector, [NotNull] IPeopleDetector detector, [NotNull] ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.denoiser = denoiser ?? throw new ArgumentNullException(nameof(denoiser));
            this.subtractor = subtractor ?? throw new ArgumentNullException(nameof(subtractor));
            this.corrector = corrector ?? throw new ArgumentNullException(nameof(corrector));
        }

        public bool? PresenceDetected
        {
            get => presenceDetected;
            private set
            {
                if (value == presenceDetected) return;
                presenceDetected = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (value == isRunning) return;
                isRunning = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Start()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(OccupancySensor));
            }

            logger.LogInformation("Start requested");

            if (!camera.IsRunning)
            {
                camera.Start();
            }

            IsRunning = true;
        }

        public void Stop()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(OccupancySensor));
            }

            logger.LogInformation("Stop requested");
            IsRunning = false;
            PresenceDetected = null;
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
                        denoiserSubscription = camera.Stream.Subscribe(denoiser);
                        subtractorSubscription= denoiser.Output.Subscribe(subtractor);
                        correctorSubscription = subtractor.Output.Subscribe(corrector);
                        detectorSubscription = corrector.Output.Subscribe(detector);
                    }
                    else
                    {
                        detectorSubscription?.Dispose();
                        correctorSubscription?.Dispose();
                        subtractorSubscription?.Dispose();
                        denoiserSubscription?.Dispose();

                        detector.Reset();
                        corrector.Reset();
                        subtractor.Reset();
                        denoiser.Reset();

                        IsRunning = false;
                        PresenceDetected = null;
                    }
                    break;
            }
        }

        private void OnDetectorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsRunning)
            {
                switch (e.PropertyName)
                {
                    case nameof(IPeopleDetector.PeopleDetected):
                        logger.LogInformation(
                            $"Detector.PeopleDetected changed to {detector.PeopleDetected?.ToString() ?? "null"}");
                        PresenceDetected = detector.PeopleDetected;
                        break;
                }
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
                    detectorSubscription?.Dispose();
                    correctorSubscription?.Dispose();
                    subtractorSubscription?.Dispose();
                    denoiserSubscription?.Dispose();

                    camera.PropertyChanged -= OnCameraPropertyChanged;
                    detector.PropertyChanged -= OnDetectorPropertyChanged;
                }

                isDisposed = true;
            }
        }
    }
}