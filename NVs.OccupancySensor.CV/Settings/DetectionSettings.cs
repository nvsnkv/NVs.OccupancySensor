using NVs.OccupancySensor.CV.Detection;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings : IDetectionSettings
    {
        public DetectionSettings(double threshold)
        {
            DetectionThreshold = threshold;
        }

        public double DetectionThreshold { get; }
        
        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d);
    }
}