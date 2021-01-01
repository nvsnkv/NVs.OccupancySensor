namespace NVs.OccupancySensor.CV
{
    public interface IResizeSettings
    {
        int TargetWidth { get; }
        
        int TargetHeight { get; }
    }
}