using System;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Denoising;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class DenoiserFactoryShould
    {
        [Fact]
        public void CreateBypassDenoiserByDefault()
        {
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default, MedianBlurDenoisingSettings.Default);
            var denoiser = factory.Create(DenoisingSettings.Default.Algorithm);

            Assert.IsType<BypassDenoiser>(denoiser);
        }

        [Theory]
        [InlineData("FastNlMeansColored", typeof(FastNlMeansColoredDenoiser))]
        [InlineData("FastNlMeans", typeof(FastNlMeansDenoiser))]
        [InlineData("MedianBlur", typeof(MedianBlurDenoiser))]
        public void CreateAppropriateDenoiserWhenRequested(string requestedType, Type expectedType)
        {
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default, MedianBlurDenoisingSettings.Default);
            var denoiser = factory.Create(requestedType);

            Assert.IsType(expectedType, denoiser);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenUnknownAlgorithmRequested()
        {
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default, MedianBlurDenoisingSettings.Default);
            Assert.Throws<ArgumentException>(() => factory.Create("Unknown"));
        }
    }
}