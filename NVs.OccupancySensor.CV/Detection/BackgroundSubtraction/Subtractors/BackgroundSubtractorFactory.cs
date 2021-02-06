using System;

namespace NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors
{
    internal sealed class BackgroundSubtractorFactory : IBackgroundSubtractorFactory
    {
        public IBackgroundSubtractor Create(string algorithm)
        {
            if (Enum.TryParse(algorithm, out SupportedAlgorithms supportedAlgorithm))
            {
                switch (supportedAlgorithm)
                {
                    case SupportedAlgorithms.CNT:
                        return new CNTSubtractor();
                }
            }

            throw new ArgumentException($"Unable to create subtractor. Unknown algorithm '{algorithm}' given", nameof(algorithm));
        }
    }
}