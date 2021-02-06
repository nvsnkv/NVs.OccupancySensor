using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    internal sealed class BackgroundSubtractionBasedDetector : IBackgroundSubtractionBasedDetector, IDisposable
    {
        private readonly IBackgroundSubtractorFactory factory;
        private readonly ILogger<BackgroundSubtractionBasedDetector> logger;
        private readonly IDecisionMaker decisionMaker;

        private volatile IBackgroundSubtractor subtractor;

        private bool? peopleDetected;
        private Image<Gray, byte> mask;
        private IBackgroundSubtractionBasedDetectorSettings settings;


        public BackgroundSubtractionBasedDetector([NotNull] IBackgroundSubtractorFactory factory, [NotNull] IDecisionMaker decisionMaker, [NotNull] ILogger<BackgroundSubtractionBasedDetector> logger, [NotNull] IBackgroundSubtractionBasedDetectorSettings settings)
        {
            this.decisionMaker = decisionMaker ?? throw new ArgumentNullException(nameof(decisionMaker));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            subtractor = this.factory.Create(Settings.Algorithm);
            this.decisionMaker.Settings = Settings;
        }
            
        public void OnCompleted()
        {
            logger.LogInformation("Stream completed, setting PeopleDetected to null");
            PeopleDetected = null;
            Mask = null;
        }

        public void OnError(Exception error)
        {
            logger.LogInformation($"Error received, setting PeopleDetected to null{Environment.NewLine}Exception: {error}");
            PeopleDetected = null;
            Mask = null;
        }

        public void OnNext([NotNull] Image<Rgb, byte> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            logger.LogInformation("New frame received...");

            Image<Gray, byte> fgMask;

            try
            {
                fgMask = subtractor.GetForegroundMask(value);
                logger.LogInformation("Background subtracted");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to calculate foreground mask!");
                throw;
            }
            
            PeopleDetected = decisionMaker.DetectPresence(fgMask);
            Mask = fgMask;
            logger.LogInformation("Properties updated.");
        }

        public void Reset()
        {
            logger.LogInformation("Reset requested...");
            PeopleDetected = null;
            Mask = null;

            logger.LogInformation("Properties reset.");

            subtractor.Dispose();
            subtractor = factory.Create(Settings.Algorithm);
            logger.LogInformation($"Subtractor recreated. Algorithm '{Settings.Algorithm}' used");
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

        public Image<Gray, byte> Mask
        {
            get => mask;
            private set
            {
                if (Equals(value, mask)) return;
                mask = value;
                OnPropertyChanged();
            }
        }

        [NotNull]
        public IBackgroundSubtractionBasedDetectorSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value ?? throw new ArgumentNullException(nameof(value)), settings)) return;
                settings = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            subtractor?.Dispose();
        }
    }
}