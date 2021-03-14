using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings : IBackgroundSubtractionBasedDetectorSettings
    {
        public DetectionSettings(double threshold, string algorithm, string correctionMask)
        {
            DetectionThreshold = threshold;
            Algorithm = algorithm;
            CorrectionMask = correctionMask;
        }

        public double DetectionThreshold { get; }
        
        public string Algorithm { get; }

        public string CorrectionMask { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d, "CNT", "None");
    }
}