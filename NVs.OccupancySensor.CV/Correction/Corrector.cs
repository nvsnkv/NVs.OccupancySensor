using System.Threading;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Correction
{
    internal sealed class Corrector : Stage, ICorrector
    {
        private readonly ICorrectionStrategyFactory factory;
        private readonly ICorrectionSettings settings;
        private readonly ICorrectionStrategyManager strategyManager;

        public Corrector(ICorrectionStrategyFactory factory, ICorrectionStrategyManager manager, ICorrectionSettings settings, ILogger<Corrector> logger) : base(logger)
        {
            this.factory = factory;
            this.strategyManager = manager;
            this.settings = settings;
            OutputStream = CreateStream();
        }

        protected override ProcessingStream CreateStream()
        {
            var strategy = factory.Create(settings.Algorithm);
            strategyManager.SetStrategy(strategy);
            return new CorrectionStream(strategy, Counter, CancellationToken.None, Logger);
        }
    }
}