using NVs.OccupancySensor.CV.Denoising;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class DenoisingSettings : IDenoisingSettings
    {
        public DenoisingSettings(string algorithm)
        {
            Algorithm = algorithm;
        }

        public string Algorithm { get; }

        public static DenoisingSettings Default { get; } = new DenoisingSettings("None");
    }
}