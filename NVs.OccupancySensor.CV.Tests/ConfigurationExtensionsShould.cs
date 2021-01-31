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

        [Theory]
        [InlineData(null)]
        [InlineData("0.12")]
        [InlineData("bad value")]
        public void ReadDetectionThresholdFromConfiguration(string threshold)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s["Threshold"]).Returns(threshold);
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Detection")).Returns(section.Object);

            if(!double.TryParse(threshold, out var expectedThreshold)) 
            {
                expectedThreshold = DetectionSettings.Default.Threshold;
            }
            
            var actual = config.Object.GetDetectorThreshold();
            Assert.Equal(expectedThreshold, actual);
        }

        [Fact]
        public void ReturnDefaultDetectionThresholdIfNoDetectionSectionExist() 
        {
            Assert.Equal(DetectionSettings.Default.Threshold, new Mock<IConfiguration>().Object.GetDetectorThreshold());
        }

        [Fact]
        public void ReturnDefaultTransformSettingsIfNoTransformSectionExist()
        {
            Assert.Equal(TransformSettings.Default, new Mock<IConfiguration>().Object.GetTransformSettings());
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData(null, null, "5")]
        [InlineData(null, "3", null)]
         [InlineData(null, "9", "7")]
        [InlineData("0.1", null, null)]
        [InlineData("0.1", null, "5")]
        [InlineData("0.1", "11", null)]
        [InlineData("0.1", "11", "5")]
        [InlineData("all", "values", "bad")]
        public void ReadTransformSettingsFromConfig(string resizeFactor, string inputBlurKernelSize, string outputBlurKernelSize)
        {
            if (!double.TryParse(resizeFactor, out double expectedResizeFactor))
            {
                expectedResizeFactor = TransformSettings.Default.ResizeFactor;
            }

            if (!int.TryParse(inputBlurKernelSize, out int expectedInputBlurKernelSize)) 
            {
                expectedInputBlurKernelSize = TransformSettings.Default.InputBlurKernelSize;
            }

            if (!int.TryParse(outputBlurKernelSize, out int expectedOutputBlurKernelSize)) 
            {
                expectedOutputBlurKernelSize = TransformSettings.Default.OutputBlurKernelSize;
            }

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s["ResizeFactor"]).Returns(resizeFactor);
            section.SetupGet(s => s["InputBlurKernelSize"]).Returns(inputBlurKernelSize);
            section.SetupGet(s => s["OutputBlurKernelSize"]).Returns(outputBlurKernelSize);

            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Transform")).Returns(section.Object);
            var settings = config.Object.GetTransformSettings();

            Assert.Equal(expectedResizeFactor, settings.ResizeFactor);
            Assert.Equal(expectedInputBlurKernelSize, settings.InputBlurKernelSize);
            Assert.Equal(expectedOutputBlurKernelSize, settings.OutputBlurKernelSize);
        }      
    }
}