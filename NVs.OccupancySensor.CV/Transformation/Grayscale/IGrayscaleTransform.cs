using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation.Grayscale
{
    internal interface IGrayscaleTransform : IDisposable
    {
        Image<Gray, byte> Apply(Image<Gray, byte> input);
    }
}