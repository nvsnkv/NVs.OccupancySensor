using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction
{
    public interface IBackgroundSubtractor : IDisposable
    {
        Image<Gray, byte> GetForegroundMask(Image<Rgb, byte> source);
    }
}