using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Observervation
{
    public interface IImageObserver : IObserver<Image<Rgb, byte>>
    {
        Task<Image<Rgb, byte>> GetImage();
    }
}