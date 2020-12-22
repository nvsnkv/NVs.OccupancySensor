using System;

namespace NVs.OccupancySensor.CV
{
    public static class DefaultSettings
    {
        public static string Source { get; } = "0";
        
        public static TimeSpan FrameInterval { get; } = TimeSpan.FromMilliseconds(100);
    }
}