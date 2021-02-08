using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IDenoiser
    {
        Image<Rgb, byte> Denoise(Image<Rgb, byte> source);
    }
}