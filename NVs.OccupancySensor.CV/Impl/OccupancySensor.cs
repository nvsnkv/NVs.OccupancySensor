using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class OccupancySensor : IOccupancySensor, IDisposable
    {
        private readonly ICamera camera;
        private readonly IMatConverter converter;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;
        private IObservable<Image<Rgb, int>> stream;

        private IDisposable subscription;
        
        private bool isDisposed;

        public OccupancySensor(ICamera camera, IMatConverter converter, IPeopleDetector detector, ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool? PresenceDetected => detector.PeopleDetected;

        public bool IsRunning => camera.IsRunning;

        public IObservable<Image<Rgb, int>> Stream
        {
            get => stream;
            private set
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(OccupancySensor));
                if (Equals(stream, value)) return;
                stream = value;
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
            GC.SuppressFinalize(this);
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
                        subscription = Stream.Subscribe(Observer.ToObserver<Image<Rgb,int>>((_) => {}));
                    }
                    else
                    {
                        subscription?.Dispose();
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