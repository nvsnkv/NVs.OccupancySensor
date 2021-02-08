using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Denoising
{
    public interface IDenoiser
    {
        Image<Rgb, byte> Denoise(Image<Rgb, byte> source);

        IFastNlMeansDenoisingSettings Settings { get; set; }
    }
}