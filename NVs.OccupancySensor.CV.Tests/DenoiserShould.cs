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
    public sealed class DenoiserShould : StageShould<Rgb,Rgb>
    {
        private readonly Denoiser denoiser;

        public DenoiserShould(): this(new Mock<ILogger<Denoiser>>(), new Mock<IDenoiserFactory>()) { }

        internal DenoiserShould(Mock<ILogger<Denoiser>> logger, Mock<IDenoiserFactory> factory): this(CreateDenoiser(logger, factory))
        {
        }

        internal DenoiserShould(Denoiser denoiser) : base(denoiser)
        {
            this.denoiser = denoiser;
        }


        [Fact]
        public async Task BypassImageIfNoDenoisingRequested()
        {
            var observer = new TestImageObserver<Rgb>();
            var expectedImage = new Image<Rgb, byte>(10, 5);
            
            using (denoiser.Output.Subscribe(observer))
            {
                await Task.Run(() => denoiser.OnNext(expectedImage));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(expectedImage, observer.ReceivedItems.Keys.First());
        }

        private static Denoiser CreateDenoiser(Mock<ILogger<Denoiser>> logger, Mock<IDenoiserFactory> factory)
        {
            factory.Setup(f => f.Create(SupportedAlgorithms.None.ToString())).Returns(new BypassDenoiser());
            return new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
        }
    }
}