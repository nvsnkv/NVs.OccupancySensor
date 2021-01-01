using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IImageResizer
    {
        Image<Rgb, float> Resize(Image<Rgb, float> input);

        IResizeSettings Settings { get; set; }
    }
}