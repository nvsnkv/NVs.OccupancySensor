namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DetectionSettings
    {
        public DetectionSettings(double threshold, string dataDir, string algorithm)
        {
            Threshold = threshold;
            DataDir = dataDir;
            Algorithm = algorithm;
        }

        public double Threshold { get; }
        
        public string DataDir { get; }

        public string Algorithm { get; }

        public static DetectionSettings Default { get; } = new DetectionSettings(0.1d, "data", "CNT");
        
    }
}