﻿using System;
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
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default);
            var denoiser = factory.Create(DenoisingSettings.Default.Algorithm);

            Assert.IsType<BypassDenoiser>(denoiser);
        }

        [Fact]
        public void CreateFastNlMeansColoredDenoiserIfRequested()
        {
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default);
            var denoiser = factory.Create("FastNlMeansColored");

            Assert.IsType<FastNlMeansColoredDenoiser>(denoiser);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenUnknownAlgorithmRequested()
        {
            var factory = new DenoiserFactory(FastNlMeansColoredDenoisingSettings.Default);
            Assert.Throws<ArgumentException>(() => factory.Create("Unknown"));
        }
    }
}