using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class CorrectorShould : StageShould<Gray, Gray>
    {
        private readonly Mock<ICorrectionStrategy> strategy;
        public CorrectorShould() : this(new Mock<ICorrectionStrategyFactory>(), new Mock<ICorrectionStrategy>(), new Mock<ICorrectionSettings>(), new Mock<ILogger<Corrector>>()) { }
        internal CorrectorShould(Mock<ICorrectionStrategyFactory> factory, Mock<ICorrectionStrategy> strategy, Mock<ICorrectionSettings> settings, Mock<ILogger<Corrector>> logger)
            : base(GetCorrector(factory, strategy, settings, logger))
        {
            this.strategy = strategy;
        }

        private static Corrector GetCorrector(Mock<ICorrectionStrategyFactory> factory, Mock<ICorrectionStrategy> strategy, Mock<ICorrectionSettings> settings, Mock<ILogger<Corrector>> logger)
        {
            strategy.Setup(s => s.Apply(It.IsAny<Image<Gray, byte>>())).Returns(new Image<Gray, byte>(1, 1));
            factory.Setup(f => f.Create(It.IsAny<string>())).Returns(strategy.Object);
            return new Corrector(factory.Object, settings.Object, logger.Object);
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