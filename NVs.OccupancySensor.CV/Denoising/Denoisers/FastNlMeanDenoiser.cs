using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class FastNlMeansDenoiser : IDenoisingStrategy
    {
        private readonly IFastNlMeansDenoisingSettings settings;

        public FastNlMeansDenoiser([NotNull] IFastNlMeansDenoisingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Image<Rgb, byte> Denoise([NotNull] Image<Rgb, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new Image<Rgb, byte>(source.Width, source.Height);
            CvInvoke.FastNlMeansDenoisingColored(source, result, settings.H, settings.HColor, settings.TemplateWindowSize, settings.SearchWindowSize);
            return result;
        }
    }
}