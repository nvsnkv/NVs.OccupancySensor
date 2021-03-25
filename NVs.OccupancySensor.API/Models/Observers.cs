using System;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using NVs.OccupancySensor.CV.Observation;

namespace NVs.OccupancySensor.API.Models
{
    public sealed class Observers
    {
        public Observers([NotNull] IImageObserver<Rgb> rgb, [NotNull] IImageObserver<Gray> gray)
        {
            Rgb = rgb ?? throw new ArgumentNullException(nameof(rgb));
            Gray = gray ?? throw new ArgumentNullException(nameof(gray));
        }

        public IImageObserver<Rgb> Rgb { get; }

        public IImageObserver<Gray> Gray { get; }
    }
}