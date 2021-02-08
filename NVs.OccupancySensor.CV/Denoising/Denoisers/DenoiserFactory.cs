using System;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Denoising.Denoisers
{
    internal sealed class DenoiserFactory
    {
        [NotNull] private IFastNlMeansDenoisingSettings fastNlMeansDenoisingSettings;

        public DenoiserFactory(IFastNlMeansDenoisingSettings fastNlMeansDenoisingSettings) => this.fastNlMeansDenoisingSettings = fastNlMeansDenoisingSettings ?? throw new ArgumentNullException(nameof(fastNlMeansDenoisingSettings));

        public IDenoiser Create([NotNull] string algorithm)
        {
            if (algorithm == null) throw new ArgumentNullException(nameof(algorithm));
            if (Enum.TryParse(algorithm, out SupportedAlgorithms supportedAlgorithm))
            {
                switch (supportedAlgorithm)
                {
                    case SupportedAlgorithms.None:
                        return new BypassDenoiser();

                    case SupportedAlgorithms.FastNlMeans:
                        return new FastNlMeansDenoiser(FastNlMeansDenoisingSettings);
                }
            }

            throw new ArgumentException($"Unable to create denoiser! Unknown algorithm '{algorithm}' given!");
        }

        [NotNull]
        public IFastNlMeansDenoisingSettings FastNlMeansDenoisingSettings
        {
            get => fastNlMeansDenoisingSettings;
            set => fastNlMeansDenoisingSettings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}