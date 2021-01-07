using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

namespace NVs.OccupancySensor.CV.Sense
{
    internal sealed class OccupancySensor : IOccupancySensor, IDisposable
    {
        private readonly ICamera camera;
        private readonly IPeopleDetector detector;
        private readonly IGrayscaleStreamTransformer transformer;
        private readonly ILogger<OccupancySensor> logger;
        
        private IObservable<Image<Gray,byte>> stream;
        private IDisposable subscription;
        private bool isDisposed;

        public OccupancySensor([NotNull] ICamera camera, [NotNull] IPeopleDetector detector, [NotNull] IGrayscaleStreamTransformer transformer, [NotNull] ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                        transformer.RebuildStreams(camera.Stream);
                        stream = transformer.OutputStreams.Last();
                        subscription = stream.Subscribe(detector);
                    }
                    else
                    {
                        subscription?.Dispose();
                        detector.Reset();
                        stream = null;
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
                    subscription?.Dispose();
                    camera.PropertyChanged -= OnCameraPropertyChanged;
                    detector.PropertyChanged -= OnDetectorPropertyChanged;
                }

                isDisposed = true;
            }
        }
    }
}