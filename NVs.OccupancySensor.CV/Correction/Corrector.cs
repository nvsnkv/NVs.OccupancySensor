using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Correction
{
    sealed class Corrector : Stage, ICorrector
    {
        private readonly ICorrectionStrategyFactory factory;
        [NotNull] private ICorrectionSettings settings;

        public Corrector([NotNull] ICorrectionStrategyFactory factory, [NotNull] ICorrectionStrategyManager manager, [NotNull] ICorrectionSettings settings, [NotNull] ILogger<Corrector> logger) : base(logger)
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
        
        [NotNull]
        public ICorrectionSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(value));
        }

        [NotNull]
        public ICorrectionStrategyManager StrategyManager { get; }
    }
}