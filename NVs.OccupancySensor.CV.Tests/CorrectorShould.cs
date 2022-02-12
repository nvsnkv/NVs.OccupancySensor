using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Correction;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class CorrectorShould : StageShould
    {
        private readonly Mock<IStatefulCorrectionStrategy> strategy = null!;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "GetCorrector implementation uses manage to verify that SetStrategy was called"), UsedImplicitly]
        private readonly Mock<ICorrectionStrategyManager> manager = null!;
        public CorrectorShould() : this(new Mock<ICorrectionStrategyFactory>(), new Mock<IStatefulCorrectionStrategy>(), new Mock<ICorrectionStrategyManager>(), new Mock<ICorrectionSettings>(), new Mock<ILogger<Corrector>>()) { }
        internal CorrectorShould(Mock<ICorrectionStrategyFactory> factory, Mock<IStatefulCorrectionStrategy> strategy, Mock<ICorrectionStrategyManager> manager, Mock<ICorrectionSettings> settings, Mock<ILogger<Corrector>> logger)
             : this(GetCorrector(factory, strategy, manager, settings, logger))
        {
            this.strategy = strategy;
            this.manager = manager;
        }

        private CorrectorShould(Corrector corrector):base(corrector)
        {
        }

        private static Corrector GetCorrector(Mock<ICorrectionStrategyFactory> factory, Mock<IStatefulCorrectionStrategy> strategy, Mock<ICorrectionStrategyManager> manager, Mock<ICorrectionSettings> settings, Mock<ILogger<Corrector>> logger)
        {
            manager.Setup(s => s.SetStrategy(strategy.Object)).Verifiable();
            strategy.Setup(s => s.Apply(It.IsAny<Image<Gray, byte>>())).Returns(new Image<Gray, byte>(1, 1));
            factory.Setup(f => f.Create(It.IsAny<string>())).Returns(strategy.Object);
            
            var result = new Corrector(factory.Object, manager.Object, settings.Object, logger.Object);

            manager.Verify();

            return result;
        }

        protected override void SetupLongRunningPayload(TimeSpan delay)
        {
            strategy.Setup(s => s.Apply(It.IsAny<Image<Gray, byte>>())).Returns(() =>
            {
                Task.Delay(delay).Wait();
                return new Image<Gray, byte>(1, 1);
            });
        }
    }
}