using NVs.OccupancySensor.CV.BackgroundSubtraction.DecisionMaking;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    public interface IBackgroundSubtractionBasedDetectorSettings : IDecisionMakerSettings
    {
        string Algorithm { get; }

        string CorrectionAlgorithm { get; }
    }
}