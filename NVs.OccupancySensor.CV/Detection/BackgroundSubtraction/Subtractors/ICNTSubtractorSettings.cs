namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    public interface ICNTSubtractorSettings
    {
        int MinPixelStability { get; }
        bool UseHistory { get; }
        int MaxPixelStability { get; }
        bool IsParallel { get; }
    }
}