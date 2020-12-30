using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class OccupancySensor : IOccupancySensor
    {
        private readonly ICamera camera;
        private readonly IMatConverter converter;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;
        private IObservable<Image<Rgb, int>> stream;

        public OccupancySensor(ICamera camera, IMatConverter converter, IPeopleDetector detector, ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool? PresenceDetected => detector.PeopleDetected;

        public bool IsRunning => camera.IsRunning;

        public IObservable<Image<Rgb, int>> Stream
        {
            get => stream;
            private set
            {
                if (Equals(stream, value)) return;
                stream = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Start()
        {
            logger.LogInformation("Start requested");
            camera.Start();
        }

        public void Stop()
        {
            logger.LogInformation("Stop requested");
            camera.Stop();
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
                        Stream = camera.Stream.Select(f => converter.Convert(f)).Select(i => detector.Detect(i));
                    }
                    else
                    {
                        Stream = null;
                        detector.Reset();
                    }
                    break;

                default:
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

                default:
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}