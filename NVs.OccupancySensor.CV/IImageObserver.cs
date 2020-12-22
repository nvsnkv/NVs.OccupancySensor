using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV
{
    public interface IImageObserver : IObserver<Mat>
    {
        Task<Image<Rgb, int>> GetImage();
    }
}