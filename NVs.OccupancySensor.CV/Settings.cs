using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("NVs.OccupancySensor.CV.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NVs.OccupancySensor.CV
{
    public sealed class Settings : ICameraSettings, IResizeSettings
    {
        public Settings(string source, TimeSpan frameInterval, int targetWidth, int targetHeight)
        {
            Source = source;
            FrameInterval = frameInterval;
            TargetWidth = targetWidth;
            TargetHeight = targetHeight;
        }

        public string Source { get; }
        
        public TimeSpan FrameInterval { get; }
        
        public int TargetWidth { get; }

        public int TargetHeight { get; }
        
        public static readonly Settings Default = new Settings("0", TimeSpan.FromMilliseconds(100), 640, 360);
    }
}