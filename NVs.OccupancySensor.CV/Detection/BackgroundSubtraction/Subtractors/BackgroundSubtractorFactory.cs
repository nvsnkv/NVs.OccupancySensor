﻿using System;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    internal sealed class BackgroundSubtractorFactory : IBackgroundSubtractorFactory
    {
        [NotNull] private ICNTSubtractorSettings cntSubtractorSettings;

        public BackgroundSubtractorFactory([NotNull] ICNTSubtractorSettings cntSubtractorSettings)
        {
            CNTSubtractorSettings = cntSubtractorSettings ?? throw new ArgumentNullException(nameof(cntSubtractorSettings));
        }

        public IBackgroundSubtractor Create(string algorithm)
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

        [NotNull]
        public ICNTSubtractorSettings CNTSubtractorSettings
        {
            get => cntSubtractorSettings;
            set => cntSubtractorSettings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}