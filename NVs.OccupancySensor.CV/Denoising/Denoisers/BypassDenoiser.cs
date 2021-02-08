using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class BypassDenoiser : IDenoiser
    {
        public Image<Rgb, byte> Denoise([NotNull] Image<Rgb, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source;
        }
    }
}