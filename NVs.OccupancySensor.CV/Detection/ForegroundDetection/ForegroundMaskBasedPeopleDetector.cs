using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Detection.ForegroundDetection;

namespace NVs.OccupancySensor.CV.Detection
{
    internal sealed class ForegroundMaskBasedPeopleDetector : IPeopleDetector
    {
        private readonly ILogger<ForegroundMaskBasedPeopleDetector> logger;
        private readonly IDecisionMaker decisionMaker;
        private bool? peopleDetected;

        public ForegroundMaskBasedPeopleDetector([NotNull] IDecisionMaker decisionMaker, [NotNull] ILogger<ForegroundMaskBasedPeopleDetector> logger)
        {
            this.decisionMaker = decisionMaker ?? throw new ArgumentNullException(nameof(decisionMaker));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    
            PeopleDetected = decisionMaker.PresenceDetected(value);
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