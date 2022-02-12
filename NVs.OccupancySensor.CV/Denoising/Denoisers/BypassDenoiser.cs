using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class BypassDenoiser : IDenoisingStrategy
    {
        public Image<Gray, byte> Denoise(Image<Gray, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source;
        }
    }
}