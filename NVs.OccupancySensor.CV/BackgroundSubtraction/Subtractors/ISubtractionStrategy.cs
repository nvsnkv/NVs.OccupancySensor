using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors
{
    public interface ISubtractionStrategy : IDisposable
    {
        Image<Gray, byte> GetForegroundMask(Image<Gray, byte> source);

        void Reset();
    }
}