using NVs.OccupancySensor.CV.Correction;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class CorrectionSettings : ICorrectionSettings
    {
        public CorrectionSettings(string algorithm)
        {
            Algorithm = algorithm;
        }

        public string Algorithm { get; }

        public static CorrectionSettings Default { get; } = new CorrectionSettings("None");
    }
}