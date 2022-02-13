using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Detection
{
    internal sealed class PeopleDetector : IPeopleDetector
    {
        private readonly ILogger<PeopleDetector> logger;

        private readonly double threshold;
        private bool? peopleDetected;
        private Image<Gray, byte>? mask;

        public PeopleDetector(IDetectionSettings settings, ILogger<PeopleDetector> logger)
        {
            this.logger = logger;
            threshold = settings.DetectionThreshold;
        }

        public void OnCompleted()
        {
            logger.LogInformation($"Stream completed, setting {nameof(PeopleDetected)} to null");
            PeopleDetected = null;
            Mask = null;
        }

        public void OnError(Exception error)
        {
            logger.LogWarning($"Received an error from the steam, setting {nameof(PeopleDetected)} to null!");
            PeopleDetected = null;
            Mask = null;
        }

        public void OnNext(Image<Gray, byte> value)
        {
            logger.LogDebug("Received new frame...");

            double average;
            try
            {
                average = value.GetAverage().Intensity / 255;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to compute average!");
                throw;
            }

            logger.LogDebug($"Computed average: {average}");
            PeopleDetected = average >= threshold;
            Mask = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public Image<Gray, byte>? Mask
        {
            get => mask;
            private set
            {
                if (Equals(value, mask)) return;
                mask = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Mask = null;
            PeopleDetected = null;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}