using System;
using System.Linq;
using System.Reactive.Linq;
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
    public sealed class GrayscaleStreamTransformerShould
    {
        private readonly Mock<ILogger<GrayscaleStreamTransformer>> logger = new Mock<ILogger<GrayscaleStreamTransformer>>();

        [Fact]
        public void ApplyTransformsToTheImageInProperOrder()
        {
            var transformA = new TestTransform();
            var transformB = new TestTransform();

            var transformer = new GrayscaleStreamTransformer(logger.Object, new[] { transformA, transformB });
            
            var inStream = Enumerable.Repeat(new Image<Rgb, byte>(10, 10), 1).ToObservable();
            transformer.RebuildStreams(inStream);

            transformer.OutputStreams.Last().Subscribe(_ => {});
            Assert.NotNull(transformA.ApplyInvokedOn);
            Assert.NotNull(transformB.ApplyInvokedOn);

            Assert.True(transformA.ApplyInvokedOn < transformB.ApplyInvokedOn);
        }

        [Fact]
        public void DisposeTransformsWhenDisposed()
        {
            var transformA = new TestTransform();
            var transformB = new TestTransform();

            var transformer = new GrayscaleStreamTransformer(logger.Object, new[] { transformA, transformB });
            transformer.Dispose();

            Assert.True(transformA.Disposed);
            Assert.True(transformB.Disposed);
        }

        [Fact]
        public void ProvideIntermediateStreams()
        {
            var transformA = new TestTransform();
            var transformB = new TestTransform();

            var transformer = new GrayscaleStreamTransformer(logger.Object, new[] { transformA, transformB });
            
            var inStream = Enumerable.Repeat(new Image<Rgb, byte>(10, 10), 1).ToObservable();
            transformer.RebuildStreams(inStream);

            transformer.OutputStreams[1].Subscribe(_ => {});
            Assert.NotNull(transformA.ApplyInvokedOn);
            Assert.Null(transformB.ApplyInvokedOn);
        }

        [Fact]
        public void ConvertImagesToGrayscaleWithoutAdditionalTransformations()
        {
            var transformer = new GrayscaleStreamTransformer(logger.Object, new IGrayscaleTransform[] {});
            
            var inStream = Enumerable.Repeat(new Image<Rgb, byte>(10, 10), 1).ToObservable();
            transformer.RebuildStreams(inStream);

            object result = null;
            transformer.OutputStreams.Last().Subscribe(i => result = i);
            Assert.IsType<Image<Gray,byte>>(result);
        }
    }
}