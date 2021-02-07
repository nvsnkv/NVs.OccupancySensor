﻿using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Subtractors;
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
    }
}