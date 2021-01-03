using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Impl
{
    public interface IImageConverter
    {
        Image<Rgb,byte> Convert(Image<Rgb,byte> input);
        
        ConversionSettings Settings { get; set; }
    }
}