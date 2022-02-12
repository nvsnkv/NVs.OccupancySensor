using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Correction
{
    sealed class Corrector : Stage, ICorrector
    {
        private readonly ICorrectionStrategyFactory factory;
        private ICorrectionSettings settings;

        public Corrector(ICorrectionStrategyFactory factory, ICorrectionStrategyManager manager, ICorrectionSettings settings, ILogger<Corrector> logger) : base(logger)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.StrategyManager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            OutputStream = CreateStream();
        }

        protected override ProcessingStream CreateStream()
        {
            var strategy = factory.Create(Settings.Algorithm);
            StrategyManager.SetStrategy(strategy);
            return new CorrectionStream(strategy, Counter, CancellationToken.None, Logger);
        }
        
                public ICorrectionSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(value));
        }

                public ICorrectionStrategyManager StrategyManager { get; }
    }
}