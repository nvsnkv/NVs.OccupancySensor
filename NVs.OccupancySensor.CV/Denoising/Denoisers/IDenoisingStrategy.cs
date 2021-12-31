using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    public interface IDenoisingStrategy
    {
        Image<Gray, byte> Denoise(Image<Gray, byte> source);
    }
}