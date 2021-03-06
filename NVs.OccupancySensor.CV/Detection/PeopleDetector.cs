﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Detection
{
    sealed class PeopleDetector : IPeopleDetector
    {
        private readonly ILogger<PeopleDetector> logger;

        private double threshold;
        private bool? peopleDetected;
        [NotNull] private IDetectionSettings settings;
        private Image<Gray, byte> mask;

        public PeopleDetector([NotNull] IDetectionSettings settings, [NotNull] ILogger<PeopleDetector> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            threshold = this.settings.DetectionThreshold;
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

        public void OnNext([NotNull] Image<Gray, byte> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            logger.LogInformation("Received new frame...");

            double average = 0;
            try
            {
                average = value.GetAverage().Intensity / 255;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to compute average!");
                throw;
            }

            logger.LogInformation($"Computed average: {average}");
            PeopleDetected = average >= threshold;
            Mask = value;
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
        public IDetectionSettings Settings
        {
            get => settings;
            set
            {
                if (Equals(value, settings)) return;
                settings = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            threshold = Settings.DetectionThreshold;
            logger.LogInformation($"Reset called. New threshold is {threshold}");

            Mask = null;
            PeopleDetected = null;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}