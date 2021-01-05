using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Tests.Utils;
using NVs.OccupancySensor.CV.Transformation;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class ImageTransformerShould
    {
        private readonly Mock<ILogger<ImageTransformer>> logger = new Mock<ILogger<ImageTransformer>>();

        [Fact]
        public void ApplyTransformsToTheImageInProperOrder()
        {
            var transformA = new TestTransform();
            var transformB = new TestTransform();

            var transformer = new ImageTransformer(logger.Object, new[] { transformA, transformB });
            transformer.Transform(new Image<Rgb, byte>(10, 10));

            Assert.NotNull(transformA.ApplyInvokedOn);
            Assert.NotNull(transformB.ApplyInvokedOn);

            Assert.True(transformA.ApplyInvokedOn < transformB.ApplyInvokedOn);
        }

        [Fact]
        public void DisposeTransformsWhenDisposed()
        {
            var transformA = new TestTransform();
            var transformB = new TestTransform();

            var transformer = new ImageTransformer(logger.Object, new[] { transformA, transformB });
            transformer.Dispose();

            Assert.True(transformA.Disposed);
            Assert.True(transformB.Disposed);
        }

        [Fact]
        public void ReturnNullForPreviousTransformerIfOnlyOneTransformationIsSetUp()
        {
            var transformer = new ImageTransformer(logger.Object, new[] { new TestTransform() });
            Assert.Null(transformer.GetPreviousTransformer());
        }

        [Fact]
        public void ReturnPreviousTransformerAsANewTransformerWithoutLastTransform()
        {
            var copyA = new TestTransform();
            var a = new TestTransform(_ => copyA);
            var b = new TestTransform();

            var transformer = new ImageTransformer(logger.Object, new[] { a, b });
            var previous = transformer.GetPreviousTransformer();

            previous.Transform(new Image<Rgb, byte>(100, 100));
            Assert.Null(a.ApplyInvokedOn);
            Assert.Null(b.ApplyInvokedOn);

            Assert.NotNull(copyA.ApplyInvokedOn);
        }

        [Fact]
        public void ThrowInvalidOperationExceptionIfLastTransformReturnedNull()
        {
            var a = new Mock<ITransform>();
            a.Setup(t => t.Apply(It.IsAny<object>())).Returns(null);

            var transformer = new ImageTransformer(logger.Object, new[] { a.Object });
            Assert.Throws<InvalidOperationException>(() => { transformer.Transform(new Image<Rgb, byte>(100, 100)); });
        }

        [Fact]
        public void ThrowInvalidOperationExceptionIfLastTransformReturnedSomethingDifferentFromImageRgbByte()
        {
            var a = new Mock<ITransform>();
            a.Setup(t => t.Apply(It.IsAny<object>())).Returns(new Image<Rgb, byte>(1, 1));

            var transformer = new ImageTransformer(logger.Object, new[] { a.Object });
            Assert.Throws<InvalidOperationException>(() => { transformer.Transform(new Image<Rgb, byte>(100, 100)); });
        }
    }
}