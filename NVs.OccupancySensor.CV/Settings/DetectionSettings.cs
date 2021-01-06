namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings
    {
        public DetectionSettings(double threshold)
        {
            Threshold = threshold;
        }

        public double Threshold { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d);
    }
}