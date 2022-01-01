using System;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Tests.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class DenoiserShould : StageShould
    {
        private readonly Mock<IDenoisingStrategy> strategy;
        private readonly Denoiser denoiser;

        public DenoiserShould(): this(new Mock<ILogger<Denoiser>>(), new Mock<IDenoiserFactory>(), new Mock<IDenoisingStrategy>()) { }

        internal DenoiserShould(Mock<ILogger<Denoiser>> logger, Mock<IDenoiserFactory> factory, Mock<IDenoisingStrategy> strategy): this(CreateDenoiser(logger, factory, strategy))
        {
            this.strategy = strategy;
        }

        internal DenoiserShould(Denoiser denoiser) : base(denoiser)
        {
            this.denoiser = denoiser;
        }

        private static Denoiser CreateDenoiser(Mock<ILogger<Denoiser>> logger, Mock<IDenoiserFactory> factory,
            Mock<IDenoisingStrategy> strategy)
        {
            strategy.Setup(s => s.Denoise(It.IsAny<Image<Gray, byte>>())).Returns(new Image<Gray, byte>(1, 1));
            factory.Setup(f => f.Create(SupportedAlgorithms.None.ToString())).Returns(strategy.Object);
            return new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
        }

        protected override void SetupLongRunningPayload(TimeSpan delay)
        {
            strategy.Setup(s => s.Denoise(It.IsAny<Image<Gray, byte>>())).Returns(() =>
            {
                Task.Delay(delay).Wait();
                return new Image<Gray, byte>(1, 1);
            });
        }
    }
}