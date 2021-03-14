using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    public interface IBackgroundSubtractionBasedDetectorSettings : IDecisionMakerSettings
    {
        string Algorithm { get; }

        string CorrectionMask { get; }
    }
}