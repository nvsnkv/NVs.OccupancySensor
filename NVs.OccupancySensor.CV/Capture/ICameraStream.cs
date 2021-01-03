using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Capture
{
    public interface ICameraStream : IObservable<Image<Rgb, byte>>
    {
    }
}