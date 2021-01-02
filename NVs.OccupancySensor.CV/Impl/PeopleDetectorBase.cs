using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    internal abstract class PeopleDetectorBase : IPeopleDetector, IDisposable
    {
        private readonly ILogger<PeopleDetectorBase> logger;
        private readonly object thisLock = new object();
        private bool? peopleDetected;
        private volatile bool processing;
        private volatile bool disposed;
        protected PeopleDetectorBase([NotNull] ILogger<PeopleDetectorBase> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public Image<Rgb, float> Detect([NotNull] Image<Rgb, float> source)
        {
            
            logger.LogInformation("Processing new image...");
            if (disposed) throw new ObjectDisposedException(nameof(IPeopleDetector));
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            var result = source.Copy();

            if (!AcquireProcessingLock())
            {
                logger.LogWarning("Previous image is still processing, received frame will be bypassed.");
                return result;
            }

            Rectangle[] regions;
            try
            {
                regions = PerformDetection(source);
                logger.LogInformation("Detection complete");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to run detection!");
                PeopleDetected = null;
                throw;
            }
            finally
            {
                ReleaseProcessingLock();
            }

            PeopleDetected = regions.Any();
            logger.LogInformation("PeopleDetected updated");

            try
            {
                foreach (var detection in regions)
                {
                    result.Draw(detection, new Rgb(Color.Magenta));
                }
                logger.LogInformation("Detected objects highlighted.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to draw rectangles!");
                throw;
            }

            return result;
        }

        protected abstract Rectangle[] PerformDetection(Image<Rgb, float> source);

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
            DoDispose(logger);
            logger.LogInformation("Dispose complete");
            disposed = true;
        }
        
        protected abstract void DoDispose(ILogger<PeopleDetectorBase> logger);

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}