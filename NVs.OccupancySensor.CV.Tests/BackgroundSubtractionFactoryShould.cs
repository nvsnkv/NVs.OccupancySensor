using System;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Subtractors;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class BackgroundSubtractionFactoryShould
    {
        [Fact]
        public void CreateCntSubtractorByDefault()
        {
            var factory = new BackgroundSubtractorFactory(CNTSubtractorSettings.Default);
            var subtractor = factory.Create(DetectionSettings.Default.Algorithm);

            Assert.IsType<CNTSubtractor>(subtractor);
        }

        [Fact]
        public void ThrowExceptionWhenUnknownAlgorithmIsRequested()
        {
            var factory = new BackgroundSubtractorFactory(CNTSubtractorSettings.Default);

            Assert.Throws<ArgumentException>(() => factory.Create("Unknown algo"));
        }
    }
}