using System.IO;
using NVs.OccupancySensor.CV.Correction;

namespace NVs.OccupancySensor.CV.Settings.Correction
{
    public sealed class StaticMaskSettings : IStaticMaskSettings
    {
        public StaticMaskSettings(string maskPath)
        {
            MaskPath = maskPath;
        }

        public string MaskPath { get; }

        public static StaticMaskSettings Default { get; } = new StaticMaskSettings(Path.Combine("data", "correction_mask.bin"));
    }
}