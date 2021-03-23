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
    public sealed class DenoiserShould
    {
        private readonly Mock<ILogger<Denoiser>> logger = new Mock<ILogger<Denoiser>>();
        private readonly Mock<IDenoiserFactory> factory = new Mock<IDenoiserFactory>();

        public DenoiserShould()
        {
            factory.Setup(f => f.Create(SupportedAlgorithms.None.ToString())).Returns(new BypassDenoiser());
        }

        [Fact]
        public async Task BypassImageIfNoDenoisingRequested()
        {
            var denoiser = new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
            var observer = new TestImageObserver();
            var expectedImage = new Image<Rgb, byte>(10, 5);
            
            using (denoiser.Output.Subscribe(observer))
            {
                await Task.Run(() => denoiser.OnNext(expectedImage));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(expectedImage, observer.ReceivedItems.Keys.First());
        }

        [Fact]
        public async Task CompleteOutputStreamWhenSourceStreamCompleted()
        {
            var denoiser = new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
            var observer = new TestImageObserver();
            var expectedImage = new Image<Rgb, byte>(10, 5);
            
            using (denoiser.Output.Subscribe(observer))
            {
                await Task.Run(() => denoiser.OnCompleted());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task ForwardErrors()
        {
            var denoiser = new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
            var observer = new TestImageObserver();
            
            using (denoiser.Output.Subscribe(observer))
            {
                await Task.Run(() => denoiser.OnError(new TestException()));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.IsType<TestException>(observer.Error);
        }

        [Fact]
        public async Task CompleteStreamOnReset()
        {
            var denoiser = new Denoiser(factory.Object, new DenoisingSettings(SupportedAlgorithms.None.ToString()), logger.Object);
            var observer = new TestImageObserver();

            using (denoiser.Output.Subscribe(observer))
            {
                await Task.Run(() => denoiser.Reset());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);

        }
    }
}