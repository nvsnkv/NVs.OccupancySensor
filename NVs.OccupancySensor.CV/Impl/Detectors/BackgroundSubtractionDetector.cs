using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl.Detectors
{
    sealed class BackgroundSubtractionDetector : IPeopleDetector, IDisposable
    {
        private readonly ILogger<BackgroundSubtractionDetector> logger;
        private readonly BackgroundSubtractorCNT subtractor;
        private readonly object thisLock = new object();
        private bool? peopleDetected;
        private volatile bool processing;
        private volatile bool disposed;
        private volatile Image<Rgb,byte> lastProcessedImage;
        
        public BackgroundSubtractionDetector([NotNull] ILogger<BackgroundSubtractionDetector> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.subtractor = new BackgroundSubtractorCNT();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool? PeopleDetected
        {
            get => peopleDetected;
            private set
            {
                if (disposed) throw new ObjectDisposedException(nameof(IPeopleDetector));
                if (value == peopleDetected) return;
                peopleDetected = value;
                OnPropertyChanged();
            }
        }

        public Image<Rgb,byte> Detect([NotNull] Image<Rgb,byte> source)
        {
            logger.LogInformation("Processing new image...");
            if (disposed) throw new ObjectDisposedException(nameof(IPeopleDetector));
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            if (!AcquireProcessingLock())
            {
                if (lastProcessedImage == null)
                {
                    logger.LogWarning("Previous image is still processing and there were no computed results so far, received frame will be bypassed.");
                    return source.Copy();
                }
                logger.LogWarning("Previous image is still processing, last processed frame will be returned.");
                return lastProcessedImage;

            }

            Image<Rgb,byte> mask = new Image<Rgb,byte>(source.Width, source.Height);
            try
            {
                subtractor.Apply(source, mask);
                logger.LogInformation("Subtraction complete");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to subtract background!");
                PeopleDetected = null;
                throw;
            }
            finally
            {
                ReleaseProcessingLock();
            }

            
            lastProcessedImage = mask;
            return mask;
        }

        public void Reset()
        {
            logger.LogInformation("Reset called");
            if (disposed) throw new ObjectDisposedException(nameof(IPeopleDetector));
            PeopleDetected = null;
        }

        private void ReleaseProcessingLock()
        {
            processing = false;
        }

        private bool AcquireProcessingLock()
        {
            if (processing)
            {
                return false;
            }

            lock (thisLock)
            {
                if (processing)
                {
                    return false;
                }

                processing = true;
                return true;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            logger.LogInformation("Dispose requested");
            
            
            
            logger.LogInformation("Dispose complete");
            disposed = true;
        }
        
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}