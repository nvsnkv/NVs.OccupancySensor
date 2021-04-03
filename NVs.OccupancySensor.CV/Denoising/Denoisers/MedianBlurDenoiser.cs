using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class MedianBlurDenoiser : IDenoisingStrategy
    {
        private readonly IMedianBlurSettings settings;

        public MedianBlurDenoiser(IMedianBlurSettings settings)
        {
            this.settings = settings;
        }

        public Image<Rgb, byte> Denoise([NotNull] Image<Rgb, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new Image<Rgb, byte>(source.Width, source.Height);

            CvInvoke.MedianBlur(source, result, settings.K);

            return result;
        }
    }
}