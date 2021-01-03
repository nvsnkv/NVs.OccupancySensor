using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation
{
    public interface IImageTransformer : IDisposable

    {
    Image<Rgb, byte> Transform(Image<Rgb, byte> input);

    IImageTransformer GetPreviousTransformer();
    }
}