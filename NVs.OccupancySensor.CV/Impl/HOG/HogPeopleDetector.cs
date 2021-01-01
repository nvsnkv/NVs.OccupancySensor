using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl.HOG
{
    sealed class HogPeopleDetector : IPeopleDetector, IDisposable
    {
        private readonly ILogger<HogPeopleDetector> logger;
        private readonly IHOGDescriptorWrapper descriptor;
        private readonly object thisLock = new object();
        
        private bool? peopleDetected;
        private volatile bool processing;
        
        public HogPeopleDetector(ILogger<HogPeopleDetector> logger, Func<IHOGDescriptorWrapper> createDescriptor)
        {
            this.logger = logger;
            descriptor = createDescriptor();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool? PeopleDetected
        {
            get => peopleDetected;
            private set
            {
                if (value == peopleDetected) return;
                peopleDetected = value;
                OnPropertyChanged();
            }
        }

        public Image<Rgb, float> Detect(Image<Rgb, float> source)
        {
            logger.LogInformation("Processing new image...");
            var result = source.Copy();

            if (!AcquireProcessingLock())
            {
                logger.LogWarning("Previous image is still processing, received frame will be bypassed.");
                return result;
            }

            MCvObjectDetection[] regions;
            try
            {
                regions = descriptor.DetectMultiScale(source);
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
                    result.Draw(detection.Rect, new Rgb(Color.Magenta));
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

        public void Reset()
        {
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
            descriptor?.Dispose();
        }

        static HOGDescriptor CreateDescriptor()
        {
            var descriptor = new HOGDescriptor();
            descriptor.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

            return descriptor;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}