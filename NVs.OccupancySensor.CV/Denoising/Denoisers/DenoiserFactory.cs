using System;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class DenoiserFactory : IDenoiserFactory
    {
        [NotNull] private IFastNlMeansColoredDenoisingSettings fastNlMeansColoredDenoisingSettings;

        public DenoiserFactory(IFastNlMeansColoredDenoisingSettings fastNlMeansColoredDenoisingSettings) => this.fastNlMeansColoredDenoisingSettings = fastNlMeansColoredDenoisingSettings ?? throw new ArgumentNullException(nameof(fastNlMeansColoredDenoisingSettings));

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
    }
}