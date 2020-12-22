using System;
using Emgu.CV;

namespace NVs.OccupancySensor.CV
{
    public interface ICamera : IObservable<Mat>
    {
    }
}