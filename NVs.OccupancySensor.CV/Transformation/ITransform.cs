using System;

namespace NVs.OccupancySensor.CV.Transformation
{
    internal interface ITransform : IDisposable
    {
        object Apply(object input);

        ITransform Clone();
    }
}