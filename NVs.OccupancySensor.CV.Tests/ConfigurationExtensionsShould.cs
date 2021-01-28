using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class ConfigurationExtensionsShould
    {
        private readonly Mock<IConfiguration> configuration = new Mock<IConfiguration>();

        [Fact]
        public void CreateDefaultCameraSettingsIfCaptureSectionIsNotPresent()
        {
            var settings = configuration.Object.GetCaptureSettings();
            Assert.Equal(CaptureSettings.Default.FrameInterval, settings.FrameInterval);
            Assert.Equal(CaptureSettings.Default.Source, settings.Source);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData("1", null)]
        [InlineData(null, "01:00:00")]
        [InlineData("2", "01:00:00")]
        [InlineData("http://some.file/", null)]
        [InlineData("http://some.file/", "05:30")]
        [InlineData("http://some.file/", "BadValue")]
        public void CreateCameraSettingsFromConfiguration(string source, string frameInterval)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s["Source"]).Returns(source);
            section.SetupGet(s => s["FrameInterval"]).Returns(frameInterval);

            configuration.Setup(c => c.GetSection("CV:Capture")).Returns(section.Object);
            
            var settings = configuration.Object.GetCaptureSettings();

            var expectedSource = source ?? CaptureSettings.Default.Source;
            if (!TimeSpan.TryParse(frameInterval, out var expectedFrameInterval))
            {
                expectedFrameInterval = CaptureSettings.Default.FrameInterval;
            }
            
            Assert.Equal(expectedFrameInterval, settings.FrameInterval);
            Assert.Equal(expectedSource, settings.Source);
        }
    }
}