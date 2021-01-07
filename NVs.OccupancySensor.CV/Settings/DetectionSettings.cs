namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings
    {
        public DetectionSettings(double threshold, string algorithmsDir)
        {
            Threshold = threshold;
            AlgorithmsDir = algorithmsDir;
        }

        public double Threshold { get; }
        
        public string AlgorithmsDir { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d, "algorithms");
        
    }
}