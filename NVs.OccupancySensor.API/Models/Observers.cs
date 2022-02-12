using System;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using NVs.OccupancySensor.CV.Observation;

namespace NVs.OccupancySensor.API.Models
{
    public sealed class Observers
    {
        public Observers(IImageObserver<Gray> gray)
        {
            Gray = gray ?? throw new ArgumentNullException(nameof(gray));
        }


        public IImageObserver<Gray> Gray { get; }
    }
}