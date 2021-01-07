using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Detection
{
    internal sealed class ForegroundMaskBasedPeopleDetector : IPeopleDetector
    {
        private readonly ILogger<ForegroundMaskBasedPeopleDetector> logger;
        private readonly double detectionTreshold;
        private bool? peopleDetected;

        public ForegroundMaskBasedPeopleDetector([NotNull] ILogger<ForegroundMaskBasedPeopleDetector> logger, double detectionTreshold)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.detectionTreshold = detectionTreshold;
        }
        
        public void OnCompleted()
        {
            logger.LogInformation("Stream completed, setting PeopleDetected to null");
            PeopleDetected = null;
        }

        public void OnError(Exception error)
        {
            logger.LogInformation($"Error received, setting PeopleDetected to null{Environment.NewLine}Exception: {error}");
            PeopleDetected = null;
        }

        public void OnNext([NotNull] Image<Gray, byte> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            logger.LogInformation("New foreground mask received");

            double result;
            try
            {
                result = value.GetAverage().Intensity / 255;
                logger.LogInformation($"Computed average: {result}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to compute average!");
                throw;
            }
            
            PeopleDetected = result > detectionTreshold;
        }

        public void Reset()
        {
            logger.LogInformation("Reset requested, setting PeopleDetected to null");
            PeopleDetected = null;
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

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}