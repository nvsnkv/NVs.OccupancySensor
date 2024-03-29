using System;

namespace NVs.OccupancySensor.CV.Correction
{
    internal sealed class CorrectionStrategyFactory : ICorrectionStrategyFactory
    {
        private IStaticMaskSettings staticMaskSettings;

        public CorrectionStrategyFactory(IStaticMaskSettings settings)
        {
            this.staticMaskSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public ICorrectionStrategy Create(string name)
        {
            if (Enum.TryParse(name, out CorrectionMask algorithm))
            {
                switch (algorithm)
                {
                    case CorrectionMask.None:
                        return new BypassCorrectionStrategy();

                    case CorrectionMask.StaticMask:
                        return new StaticMaskCorrectionStrategy(StaticMaskSettings);
                }
            }

            throw new ArgumentException("Unknown correction algorithm given!", nameof(name))
            {
                Data = { { "Requested algorithm", name } }
            };
        }

                public IStaticMaskSettings StaticMaskSettings
        {
            get => staticMaskSettings;
            set => staticMaskSettings = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}