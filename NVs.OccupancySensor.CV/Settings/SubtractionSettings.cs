using NVs.OccupancySensor.CV.BackgroundSubtraction;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class SubtractionSettings : IBackgroundSubtractorSettings
    {
        public SubtractionSettings(string algorithm)
        {
            Algorithm = algorithm;
        }

        public string Algorithm { get; }

        public static SubtractionSettings Default { get; } = new SubtractionSettings("CNT");
    }
}