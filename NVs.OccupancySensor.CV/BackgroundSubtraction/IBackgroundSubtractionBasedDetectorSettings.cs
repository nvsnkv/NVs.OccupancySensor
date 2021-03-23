using System;
using NVs.OccupancySensor.CV.Detection.DecisionMaking;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    [Obsolete]
    public interface IBackgroundSubtractionBasedDetectorSettings : IDecisionMakerSettings, IBackgroundSubtractorSettings
    {
        string CorrectionAlgorithm { get; }
    }
}