using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Detection.ForegroundDetection
{
    internal sealed class DecisionMaker : IDecisionMaker
    {
        private readonly ILogger<DecisionMaker> logger;
        private IDecisionMakerSettings settings;

        public DecisionMaker([NotNull] ILogger<DecisionMaker> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [NotNull]
        public IDecisionMakerSettings Settings { get => settings; set => settings = value ?? throw new ArgumentNullException(nameof(value)); }

        public bool PresenceDetected(Image<Gray, byte> mask)
        {
            double average = 0;
            try
            {
                average = mask.GetAverage().Intensity / 255;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to compute average!");
                throw;
            }

            logger.LogInformation($"Computed average {average}");
            return average >= Settings.DetectionThreshold;
        }
    }
}