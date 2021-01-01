using System;

namespace NVs.OccupancySensor.CV
{
    public interface ICameraSettings
    {
        string Source { get; }
        TimeSpan FrameInterval { get; }
    }
}