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
            CorrectionAlgorithm = correctionMask;
        }

        public double DetectionThreshold { get; }
        
        public string Algorithm { get; }

        public string CorrectionAlgorithm { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d, "CNT", "None");
    }
}