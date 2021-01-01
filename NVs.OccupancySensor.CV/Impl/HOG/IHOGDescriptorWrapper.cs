using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Impl.HOG
{
    interface IHOGDescriptorWrapper : IDisposable 
    {
        MCvObjectDetection[] DetectMultiScale(Image<Rgb, float> source);
    }
}