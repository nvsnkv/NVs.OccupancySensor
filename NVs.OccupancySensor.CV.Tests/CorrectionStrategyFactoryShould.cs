using System;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Correction;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class CorrectionStrategyFactoryShould
    {
        private readonly CorrectionStrategyFactory factory = new CorrectionStrategyFactory(StaticMaskSettings.Default);

        [Fact]
        public void CreateBypassCorrectorByDefault()
        {
            Assert.IsType<BypassCorrectionStrategy>(factory.Create(CorrectionSettings.Default.Algorithm));
        }

        [Theory]
        [InlineData("None", typeof(BypassCorrectionStrategy))]

        public void CreateAppropriateDenoiserWhenRequested(string requestedType, Type expectedType)
        {
            Assert.IsType(expectedType, factory.Create(requestedType));
        }

        [Fact]
        public void ThrowArgumentExceptionWhenUnknownAlgorithmRequested()
        {
            Assert.Throws<ArgumentException>(() => factory.Create("Unknown"));
        }
    }
}