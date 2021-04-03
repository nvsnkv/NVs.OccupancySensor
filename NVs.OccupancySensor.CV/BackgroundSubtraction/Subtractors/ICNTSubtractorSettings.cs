namespace NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors
{
    public interface ICNTSubtractorSettings
    {
        int MinPixelStability { get; }
        bool UseHistory { get; }
        int MaxPixelStability { get; }
        bool IsParallel { get; }
    }
}