using System;

namespace NVs.OccupancySensor.CV.Transformation
{
    internal interface ITypedTransform : ITransform
    {
        Type InType { get; }

        Type OutType { get; }
    }
}