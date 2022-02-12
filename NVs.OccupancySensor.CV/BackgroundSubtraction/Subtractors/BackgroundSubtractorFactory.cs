using System;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors
{
    internal sealed class BackgroundSubtractorFactory : IBackgroundSubtractorFactory
    {
        private ICNTSubtractorSettings cntSubtractorSettings;

        public BackgroundSubtractorFactory(ICNTSubtractorSettings cntSubtractorSettings)
        {
            this.cntSubtractorSettings = cntSubtractorSettings ?? throw new ArgumentNullException(nameof(cntSubtractorSettings));
        }

        public ISubtractionStrategy Create(string algorithm)
        {
            if (Enum.TryParse(algorithm, out SupportedAlgorithms supportedAlgorithm))
            {
                switch (supportedAlgorithm)
                {
                    case SupportedAlgorithms.CNT:
                        return new CNTSubtractor(CNTSubtractorSettings);
                }
            }

            throw new ArgumentException($"Unable to create subtractor. Unknown algorithm '{algorithm}' given", nameof(algorithm));
        }

                public ICNTSubtractorSettings CNTSubtractorSettings
        {
            get => cntSubtractorSettings;
            set => cntSubtractorSettings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}