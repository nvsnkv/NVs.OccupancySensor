using System;

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class CaptureSettings
    {
        public CaptureSettings(string source, TimeSpan frameInterval)
        {
            Source = source;
            FrameInterval = frameInterval;
        }

        public string Source { get; }

        public TimeSpan FrameInterval { get; }

        public static CaptureSettings Default { get; } = new CaptureSettings("0", TimeSpan.FromMilliseconds(100));
    }
}