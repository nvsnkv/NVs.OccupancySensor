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
        private readonly IMatConverter matConverter;
        private readonly IImageConverter imageConverter;
        private readonly IPeopleDetector detector;
        private readonly ILogger<OccupancySensor> logger;
        private readonly IObserver<Image<Rgb,byte>> dummyObserver = Observer.ToObserver<Image<Rgb,byte>>((_) => {});
        
        private IObservable<Image<Rgb,byte>> stream;
        private IDisposable subscription;
        private bool isDisposed;

        public OccupancySensor([NotNull] ICamera camera, [NotNull] IMatConverter matConverter, [NotNull] IImageConverter imageConverter, [NotNull] IPeopleDetector detector, [NotNull] ILogger<OccupancySensor> logger)
        {
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.camera.PropertyChanged += OnCameraPropertyChanged;

            this.matConverter = matConverter ?? throw new ArgumentNullException(nameof(matConverter));
            this.imageConverter = imageConverter ?? throw new ArgumentNullException(nameof(imageConverter));
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
            this.detector.PropertyChanged += OnDetectorPropertyChanged;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool? PresenceDetected => detector.PeopleDetected;

        public bool IsRunning => camera.IsRunning;

        public IObservable<Image<Rgb,byte>> Stream
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
                        Stream = camera.Stream
                            .Select(f => matConverter.Convert(f))
                            .Select(i => imageConverter.Convert(i))
                            .Select(i => detector.Detect(i));
                        subscription = Stream.Subscribe(dummyObserver);
                    }
                    else
                    {
                        subscription?.Dispose();
                        Stream = null;
                        detector.Reset();
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