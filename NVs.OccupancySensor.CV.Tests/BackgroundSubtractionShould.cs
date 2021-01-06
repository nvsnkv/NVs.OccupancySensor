using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Transformation.Background;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class BackgroundSubtractionShould
    {
        private const string SubtractorName = "the SubtractorName";
        private readonly Mock<ILogger<BackgroundSubtraction>> logger = new Mock<ILogger<BackgroundSubtraction>>();
        private readonly Mock<IAlgorithmModelStorage> storage = new Mock<IAlgorithmModelStorage>();

        [Fact]
        public void LoadAlgorithmDuringCreation()
        {
            
            storage.Setup(s => s.GetAlgorithm(It.Is<string>(n => SubtractorName.Equals(n))))
                .Returns((string)null)
                .Verifiable("Load was not called!");

            var _ = new BackgroundSubtraction(storage.Object, logger.Object, SubtractorName);
            
            storage.Verify();
        }

        [Fact]
        public void SaveAlgorithmOnceModelUpdated()
        {
            storage.Setup(s => s.SaveAlgorithm(It.Is<string>(n => SubtractorName.Equals(n)), It.IsAny<string>()))
                .Verifiable("Save was not called!");

            var transform = new BackgroundSubtraction(storage.Object, logger.Object, SubtractorName);
            
            transform.ResetModel();
            var counter = 0;
            while (counter++ < 100)
            {
                transform.Apply(new Image<Gray, byte>(10, 10));
            }
            
            storage.Verify();
        }
    }
}