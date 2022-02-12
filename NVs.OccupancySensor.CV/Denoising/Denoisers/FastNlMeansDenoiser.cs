using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class FastNlMeansDenoiser : IDenoisingStrategy
    {
        private readonly IFastNlMeansDenoisingSettings settings;

        public FastNlMeansDenoiser(IFastNlMeansDenoisingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Image<Gray, byte> Denoise(Image<Gray, byte> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new Image<Gray, byte>(source.Width, source.Height);
            CvInvoke.FastNlMeansDenoising(source, result, settings.H, settings.TemplateWindowSize, settings.SearchWindowSize);
            return result;
        }
    }
}