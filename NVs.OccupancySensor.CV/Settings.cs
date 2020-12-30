using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("NVs.OccupancySensor.CV.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NVs.OccupancySensor.CV
{
    public sealed class Settings
    {
        public Settings(string source, TimeSpan frameInterval)
        {
            Source = source;
            FrameInterval = frameInterval;
        }

        public string Source { get; }
        
        public TimeSpan FrameInterval { get; }

        public static readonly Settings Default = new Settings("0", TimeSpan.FromMilliseconds(100));
    }
}