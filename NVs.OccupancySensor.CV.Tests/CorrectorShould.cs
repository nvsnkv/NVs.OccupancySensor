using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Correction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class CorrectorShould : StageShould
    {
        private readonly Corrector corrector;
        private readonly Mock<IStatefulCorrectionStrategy> strategy;
        private readonly Mock<ICorrectionStrategyManager> manager;
        public CorrectorShould() : this(new Mock<ICorrectionStrategyFactory>(), new Mock<IStatefulCorrectionStrategy>(), new Mock<ICorrectionStrategyManager>(), new Mock<ICorrectionSettings>(), new Mock<ILogger<Corrector>>()) { }
        internal CorrectorShould(Mock<ICorrectionStrategyFactory> factory, Mock<IStatefulCorrectionStrategy> strategy, Mock<ICorrectionStrategyManager> manager, Mock<ICorrectionSettings> settings, Mock<ILogger<Corrector>> logger)
             : this(GetCorrector(factory, strategy, manager, settings, logger))
        {
            this.strategy = strategy;
            this.manager = manager;
        }

        internal CorrectorShould(Corrector corrector):base(corrector)
        {
            this.corrector = corrector;
        }

        [Fact]
        public void ConnectStrategyWithManager()
        {
            manager.Setup(s => s.SetStrategy(strategy.Object)).Verifiable();
            corrector.Reset();

            manager.Verify();
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