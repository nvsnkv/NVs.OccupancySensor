using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Tests.Utils;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Grayscale;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class GrayscaleStreamTransformerBuilderShould
    {
        private readonly Mock<ILogger<GrayscaleStreamTransformer>> logger = new Mock<ILogger<GrayscaleStreamTransformer>>();

        private ILogger<GrayscaleStreamTransformer> GetLogger() => logger.Object;

        [Fact]
        public void BuildTransformerWithCorrectSequenceOfActions()
        {
            DateTime? invokedA = null;
            DateTime? invokedB = null;

            var transformer = new GrayscaleStreamTransformerBuilder(GetLogger)
                .Append(_ =>
                {
                    invokedA = DateTime.Now;
                    return new Image<Gray, byte>(100, 100);
                }).Append(_ =>
                {
                    invokedB = DateTime.Now;
                    return new Image<Gray, byte>(100, 100);
                }).ToTransformer();

            transformer.RebuildStreams(Enumerable.Repeat(new Image<Rgb,byte>(100, 100), 1).ToObservable());
            transformer.OutputStreams.Last().Subscribe(_ => {});

            Assert.NotNull(invokedA);
            Assert.NotNull(invokedB);

            Assert.True(invokedA < invokedB);
        }

        private volatile int seq = 0;

        [Fact]
        public async Task AddSynchronizationWhenRequested()
        {

            Image<Gray, byte> LongRunningTransform(Image<Gray, byte> src)
            {
                Task.Delay(200).Wait();
                var image = new Image<Gray, byte>(10, 10)
                {
                    Data = { [0, 0, 0] = (byte)Interlocked.Increment(ref seq) }
                };

                return image;
            }

            var builder = new GrayscaleStreamTransformerBuilder(GetLogger).Append(LongRunningTransform).Synchronized();
            var transformer = builder.ToTransformer();

            transformer.RebuildStreams(new TestObservable<Image<Rgb, byte>>(Enumerable.Repeat(new Image<Rgb, byte>(10, 10), 3).ToList(), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(1500)));
            var results = await transformer.OutputStreams.Last().ToList();
            Assert.Equal(1, results[0].Data[0, 0, 0]);
            Assert.Equal(1, results[1].Data[0, 0, 0]);
            Assert.Equal(1, results[2].Data[0, 0, 0]);
        }
    }
}