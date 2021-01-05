using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Transformation;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public class TransformBuilderShould
    {
        private readonly Mock<ILogger<ImageTransformer>> logger = new Mock<ILogger<ImageTransformer>>();

        private ILogger<ImageTransformer> GetLogger() => logger.Object;

        [Fact]
        public void BuildTransformerWithCorrectSequenceOfActions()
        {
            DateTime? invokedA = null;
            DateTime? invokedB = null;

            var transformer = new ImageTransformBuilder(GetLogger)
                .Append((Image<Rgb, byte> _) =>
                {
                    invokedA = DateTime.Now;
                    return new Image<Rgba, byte>(100, 100);
                }).Append((Image<Rgba, byte> _) =>
                {
                    invokedB = DateTime.Now;
                    return new Image<Gray, byte>(100, 100);
                }).ToTransformer();

            transformer.Transform(new Image<Rgb, byte>(100, 100));

            Assert.NotNull(invokedA);
            Assert.NotNull(invokedB);

            Assert.True(invokedA < invokedB);
        }

        [Fact]
        public void ThrowInvalidOperationExceptionIfTransformTypesDoesNotMatch()
        {
            var builder = new ImageTransformBuilder(GetLogger)
                .Append((Image<Rgb, byte> _) => new Image<Gray, byte>(100, 100));

            Assert.Throws<InvalidOperationException>(() =>
                builder.Append((Image<Rgb, byte> _) => new Image<Rgb, byte>(100, 100)));
        }

        private volatile int seq = 0;

        [Fact]
        public void AddSynchronizationWhenRequested()
        {

            Image<Gray, byte> LongRunningTransform(Image<Rgb, byte> src)
            {
                Task.Delay(200).Wait();
                var image = new Image<Gray, byte>(10, 10)
                {
                    Data = { [0, 0, 0] = (byte)Interlocked.Increment(ref seq) }
                };

                return image;
            }

            var builder = new ImageTransformBuilder(GetLogger).Append((Func<Image<Rgb, byte>, Image<Gray, byte>>)LongRunningTransform).Synchronized();
            var transformer = builder.ToTransformer();

            var results = Enumerable.Range(0, 3)
                .AsParallel()
                .Select(_ => transformer.Transform(new Image<Rgb, byte>(1, 1)))
                .ToList();

            Assert.Equal(1, results[0].Data[0, 0, 0]);
            Assert.Equal(1, results[1].Data[0, 0, 0]);
            Assert.Equal(1, results[2].Data[0, 0, 0]);
        }
    }
}