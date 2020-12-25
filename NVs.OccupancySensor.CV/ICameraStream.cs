using System;
using Emgu.CV;

namespace NVs.OccupancySensor.CV
{
    public interface ICameraStream : IObservable<Mat>
    {
    }
}