using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Denoising;
using NVs.OccupancySensor.CV.Settings.Subtractors;
using NVs.OccupancySensor.CV.Utils;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public sealed class ConfigurationExtensionsShould
    {
        [Fact]
        public void CreateDefaultCameraSettingsIfCaptureSectionIsNotPresent()
        {
            var config = new Mock<IConfiguration>();
            var settings = config.Object.GetCaptureSettings();
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

            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Capture")).Returns(section.Object);
            
            var settings = config.Object.GetCaptureSettings();

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
                expectedThreshold = DetectionSettings.Default.DetectionThreshold;
            }
            
            var actual = config.Object.GetDetectionSettings().DetectionThreshold;
            Assert.Equal(expectedThreshold, actual);
        }

        [Fact]
        public void ReturnDefaultDetectionThresholdIfNoDetectionSectionExist() 
        {
            Assert.Equal(DetectionSettings.Default.DetectionThreshold, new Mock<IConfiguration>().Object.GetDetectionSettings().DetectionThreshold);
        }

        [Fact]
        public void ReturnDetectionAlgorithmFromSettings()
        {
            var section = new Mock<IConfigurationSection>();
            var expectedAlgorithm = "CNT";
            section.SetupGet(s => s["Algorithm"]).Returns(expectedAlgorithm);
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Detection")).Returns(section.Object);

            var actual = config.Object.GetDetectionSettings().Algorithm;

            Assert.Equal(expectedAlgorithm, actual);
        }

        [Fact]
        public void ReturnDefaultDetectionAlgorithmIfSettingsWereNotProvided()
        {
            var section = new Mock<IConfigurationSection>();
            var expectedAlgorithm = "CNT";
            section.SetupGet(s => s["Algorithm"]).Returns((string)null);
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Detection")).Returns(section.Object);

            var actual = config.Object.GetDetectionSettings().Algorithm;

            Assert.Equal(expectedAlgorithm, actual);
        }

        [Fact]
        public void ReturnDefaultDetectionAlgorithmIfSettingsSectionIsNotProvided()
        {
            var expectedAlgorithm = "CNT";
            var config = new Mock<IConfiguration>();
            

            var actual = config.Object.GetDetectionSettings().Algorithm;

            Assert.Equal(expectedAlgorithm, actual);
        }

        [Theory]
        [InlineData(false, null, null, null, null)]
        [InlineData(true, null, null, null, null)]
        [InlineData(true, "totally", "invalid", "configuration", "given")]
        [InlineData(true, "20", "False", "800", "False")]
        public void ReturnSettingsForCNTSubtractor(bool sectionExists, string minPixel, string history, string maxPixel, string parallel)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s["MinPixelStability"]).Returns(minPixel);
            section.SetupGet(s => s["UseHistory"]).Returns(history);
            section.SetupGet(s => s["MaxPixelStability"]).Returns(maxPixel);
            section.SetupGet(s => s["IsParallel"]).Returns(parallel);

            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Detection:CNT")).Returns(sectionExists ? section.Object : null);

            var expectedMinPixel = sectionExists && int.TryParse(minPixel, out var mps) ? mps : CNTSubtractorSettings.Default.MinPixelStability;
            var expectedMaxPixel = sectionExists && int.TryParse(maxPixel, out var maps) ? maps : CNTSubtractorSettings.Default.MaxPixelStability;
            var expectedUseHistory = sectionExists && bool.TryParse(history, out var h) ? h : CNTSubtractorSettings.Default.UseHistory;
            var expectedIsParallel = sectionExists && bool.TryParse(parallel, out var p) ? p : CNTSubtractorSettings.Default.IsParallel;

            var actual = config.Object.GetCNTSubtractorSettings();

            Assert.Equal(expectedMinPixel, actual.MinPixelStability);
            Assert.Equal(expectedMaxPixel, actual.MaxPixelStability);
            Assert.Equal(expectedUseHistory, actual.UseHistory);
            Assert.Equal(expectedIsParallel, actual.IsParallel);
        }

        [Theory]
        [InlineData(false, null, null, null, null)]
        [InlineData(true, null, null, null, null)]
        [InlineData(true, "totally", "invalid", "configuration", "given")]
        [InlineData(true, "5", "5", "9", "31")]
        [InlineData(true, "0.1", "9.2", "9", "31")]
        public void ReturnSettingsForFastNlMeansColoredDenoiser(bool sectionExists, string h, string hColor, string templateWindowSize, string searchWindowSize)
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s["H"]).Returns(h);
            section.SetupGet(s => s["HColor"]).Returns(hColor);
            section.SetupGet(s => s["TemplateWindowSize"]).Returns(templateWindowSize);
            section.SetupGet(s => s["SearchWindowSize"]).Returns(searchWindowSize);

            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetSection("CV:Denoising:FastNlMeans")).Returns(sectionExists ? section.Object : null);

            var expectedH = float.TryParse(h, out var th) ? th : FastNlMeansColoredDenoisingSettings.Default.H;
            var expectedHColor = float.TryParse(hColor, out var thColor) ? thColor : FastNlMeansColoredDenoisingSettings.Default.HColor;
            var expectedTemplateWindowSize = int.TryParse(templateWindowSize, out var ttemplateWindowSize) ? ttemplateWindowSize : FastNlMeansColoredDenoisingSettings.Default.TemplateWindowSize;
            var expectedSearchWindowSize = float.TryParse(searchWindowSize, out var tsearchWindowSize) ? tsearchWindowSize : FastNlMeansColoredDenoisingSettings.Default.SearchWindowSize;

            var actual = config.Object.GetFastNlMeansColoredDenoisingSettings();

            Assert.Equal(expectedH, actual.H);
            Assert.Equal(expectedHColor, actual.HColor);
            Assert.Equal(expectedSearchWindowSize, actual.SearchWindowSize);
            Assert.Equal(expectedTemplateWindowSize, actual.TemplateWindowSize);
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(true, "")]
        [InlineData(true, "None")]
        [InlineData(true, "FastNlMeansColored")]
        [InlineData(true, "banana")]
        public void ReturnSettingsForDenoiser(bool sectionExists, string algorithm)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s["Algorithm"]).Returns(algorithm);

            var config = new Mock<IConfiguration>();
            config.Setup(s => s.GetSection("CV:Denoising")).Returns(sectionExists ? section.Object : null);

            var expectedAlgorithm = (sectionExists ? algorithm : null) ?? DenoisingSettings.Default.Algorithm;
            Assert.Equal(expectedAlgorithm, config.Object.GetDenoisingSettings().Algorithm);
        }

    }
}