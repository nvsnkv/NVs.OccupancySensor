using NVs.OccupancySensor.CV.Detection.ForegroundDetection;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings : IDecisionMakerSettings
    {
        public DetectionSettings(double threshold, string dataDir, string algorithm)
        {
            DetectionThreshold = threshold;
            DataDir = dataDir;
            Algorithm = algorithm;
        }

        public double DetectionThreshold { get; }
        
        public string DataDir { get; }

        public string Algorithm { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d, "data", "CNT");
    }
}