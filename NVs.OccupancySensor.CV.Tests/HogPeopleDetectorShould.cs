using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Impl.Detectors.HOG;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class HogPeopleDetectorShould
    {
        private readonly Mock<ILogger<HogPeopleDetector>> logger;

        public HogPeopleDetectorShould()
        {
            logger = new Mock<ILogger<HogPeopleDetector>>(MockBehavior.Loose);
        }

        [Fact]
        public void DisposeDescriptorWhenDisposed()
        {
            var descriptorMock = new Mock<IHOGDescriptorWrapper>();
            descriptorMock.Setup(d => d.Dispose()).Verifiable("Dispose was not called!");
            
            var detector = new HogPeopleDetector(logger.Object, () => descriptorMock.Object);
            
            detector.Dispose();
            descriptorMock.Verify();
        }
    }
}