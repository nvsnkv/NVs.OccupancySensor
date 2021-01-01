using System;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class CameraSettings
    {
        public CameraSettings(string source, TimeSpan frameInterval)
        {
            Source = source;
            FrameInterval = frameInterval;
        }

        public string Source { get; }

        public TimeSpan FrameInterval { get; }

        public static CameraSettings Default { get; } = new CameraSettings("0", TimeSpan.FromMilliseconds(100));
    }
}