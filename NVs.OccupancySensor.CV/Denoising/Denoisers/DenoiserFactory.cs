using System;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class DenoiserFactory : IDenoiserFactory
    {
        [NotNull] private IFastNlMeansColoredDenoisingSettings fastNlMeansColoredDenoisingSettings;
        [NotNull] private IMedianBlurSettings medianBlurSettings;

        public DenoiserFactory(IFastNlMeansColoredDenoisingSettings fastNlMeansColoredDenoisingSettings, [NotNull] IMedianBlurSettings medianBlurSettings)
        {
            this.fastNlMeansColoredDenoisingSettings = fastNlMeansColoredDenoisingSettings ?? throw new ArgumentNullException(nameof(fastNlMeansColoredDenoisingSettings));
            this.medianBlurSettings = medianBlurSettings ?? throw new ArgumentNullException(nameof(medianBlurSettings));
        }

        public IDenoisingStrategy Create([NotNull] string algorithm)
        {
            if (algorithm == null) throw new ArgumentNullException(nameof(algorithm));
            if (Enum.TryParse(algorithm, out SupportedAlgorithms supportedAlgorithm))
            {
                switch (supportedAlgorithm)
                {
                    case SupportedAlgorithms.None:
                        return new BypassDenoiser();

                    case SupportedAlgorithms.FastNlMeansColored:
                        return new FastNlMeansColoredDenoiser(FastNlMeansColoredDenoisingSettings);

                    case SupportedAlgorithms.FastNlMeans:
                        return new FastNlMeansDenoiser(FastNlMeansColoredDenoisingSettings);

                    case SupportedAlgorithms.MedianBlur:
                        return new MedianBlurDenoiser(MedianBlurSettings);
                }
            }

            throw new ArgumentException($"Unable to create denoiser! Unknown algorithm '{algorithm}' given!");
        }

        [NotNull]
        public IFastNlMeansColoredDenoisingSettings FastNlMeansColoredDenoisingSettings
        {
            get => fastNlMeansColoredDenoisingSettings;
            set => fastNlMeansColoredDenoisingSettings = value ?? throw new ArgumentNullException(nameof(value));
        }

        [NotNull]
        public IMedianBlurSettings MedianBlurSettings
        {
            get => medianBlurSettings;
            set => medianBlurSettings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}