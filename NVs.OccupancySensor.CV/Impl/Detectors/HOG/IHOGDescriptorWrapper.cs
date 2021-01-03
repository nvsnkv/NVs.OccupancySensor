using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Impl.Detectors.HOG
{
    interface IHOGDescriptorWrapper : IDisposable 
    {
        MCvObjectDetection[] DetectMultiScale(Image<Rgb,byte> source);
    }
}