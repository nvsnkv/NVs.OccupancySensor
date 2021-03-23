namespace NVs.OccupancySensor.CV.Utils
{
    public interface IStatistics
    {
        ulong ProcessedFrames { get; }

        ulong DroppedFrames { get; }

        ulong Errors { get; }
    }
}