using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking;
using NVs.OccupancySensor.CV.Settings;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    
    public sealed class DecisionMakerShould
    {
        private sealed class TestSettings : IDecisionMakerSettings
        {
            public TestSettings(double detectionTreshold)
            {
                DetectionThreshold = detectionTreshold;
            }

            public double DetectionThreshold { get; }
        }

        private readonly Mock<ILogger<DecisionMaker>> logger = new Mock<ILogger<DecisionMaker>>(); 

        [Fact]
        public void DetectPrecenceIfMaskIsWhite()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            };

            var decisionMaker = new DecisionMaker(logger.Object) { Settings = DetectionSettings.Default };
            var result = decisionMaker.DetectPresence(image);

            Assert.True(result);
        }

        [Fact]
        public void NotDetectPresenceIfMaskIsBlack()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(0);
            };

            var decisionMaker = new DecisionMaker(logger.Object) { Settings = DetectionSettings.Default };
            var result = decisionMaker.DetectPresence(image);

            Assert.False(result);
        }

        [Fact]
        public void RespectSettingsChange()
        {
            var image = new Image<Gray, byte>(10, 10);
            for(var i=0; i < image.Width; i++)
            for(var j=0; j < image.Height; j++)
            {
                image[i,j] = new Gray(255);
            };

            var decisionMaker = new DecisionMaker(logger.Object) { Settings = DetectionSettings.Default };
            var result = decisionMaker.DetectPresence(image);

            Assert.True(result);

            decisionMaker.Settings = new TestSettings(1.1);
            result = decisionMaker.DetectPresence(image);

            Assert.False(result);

        }
    }
}