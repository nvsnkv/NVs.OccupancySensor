using System.Drawing;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("NVs.OccupancySensor.CV.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NVs.OccupancySensor.CV.Settings
{
    public sealed class ConversionSettings
    {
        public ConversionSettings(Size? targetSize, bool grayScale, double rotationAngle)
        {
            TargetSize = targetSize;
            GrayScale = grayScale;
            RotationAngle = rotationAngle;
        }

        public bool Resize => TargetSize.HasValue;
        
        public Size? TargetSize { get; }
        
        public bool GrayScale { get; }
        
        public double RotationAngle { get; }

        public static ConversionSettings Default { get; } = new ConversionSettings(new Size(640, 360), true, 90);
    }
}