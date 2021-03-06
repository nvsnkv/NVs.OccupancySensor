using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors;

namespace NVs.OccupancySensor.CV.Settings.Subtractors
{
    public sealed class CNTSubtractorSettings : ICNTSubtractorSettings
    {
        public CNTSubtractorSettings(int minPixelStability, bool useHistory, int maxPixelStability, bool isParallel)
        {
            MinPixelStability = minPixelStability;
            UseHistory = useHistory;
            MaxPixelStability = maxPixelStability;
            IsParallel = isParallel;
        }

        public bool UseHistory { get; }
        public int MaxPixelStability { get; }
        public bool IsParallel { get; }
        public int MinPixelStability { get; }

        public static CNTSubtractorSettings Default = new CNTSubtractorSettings(15, true, 900, true);
    }
}