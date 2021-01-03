using System;
using System.Runtime.InteropServices.WindowsRuntime;
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
                    return new Image<Gray, byte>(100, 100);
                }).Append((Image<Gray, byte> _) =>
                {
                    invokedB = DateTime.Now;
                    return new Image<Rgb, byte>(100, 100);
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
    }
}