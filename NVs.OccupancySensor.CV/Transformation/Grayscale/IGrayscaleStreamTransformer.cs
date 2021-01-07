using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Transformation.Grayscale
{
    public interface IGrayscaleStreamTransformer : IDisposable 
    {
        IObservable<Image<Rgb, byte>> InputStream { get; }

        IReadOnlyList<IObservable<Image<Gray, byte>>> OutputStreams { get; }

        void RebuildStreams(IObservable<Image<Rgb, byte>> input);
    }
}
