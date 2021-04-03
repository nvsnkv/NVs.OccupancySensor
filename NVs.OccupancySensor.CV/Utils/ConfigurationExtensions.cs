using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Settings.Correction;
using NVs.OccupancySensor.CV.Settings.Denoising;
using NVs.OccupancySensor.CV.Settings.Subtractors;

namespace NVs.OccupancySensor.CV.Utils
{
    public static class ConfigurationExtensions
    {
        public static CaptureSettings GetCaptureSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var cvSource = config.GetSection("CV:Capture")?["Source"] ?? CaptureSettings.Default.Source;
            var cvFrameInterval = config.GetSection("CV:Capture")?["FrameInterval"] ?? string.Empty;

            if (!TimeSpan.TryParse(cvFrameInterval, out TimeSpan frameInterval))
            {
                frameInterval = CaptureSettings.Default.FrameInterval;
            }

            var cameraSettings = new CaptureSettings(cvSource, frameInterval);
            return cameraSettings;
        }

        public static FastNlMeansColoredDenoisingSettings GetFastNlMeansColoredDenoisingSettings([NotNull] this IConfiguration config)
        {
            if (config is null)  throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Denoising:FastNlMeans");

            return new FastNlMeansColoredDenoisingSettings(
                float.TryParse(section?["H"], out var h) ? h : FastNlMeansColoredDenoisingSettings.Default.H,
                float.TryParse(section?["HColor"], out var hColor) ? hColor : FastNlMeansColoredDenoisingSettings.Default.HColor,
                int.TryParse(section?["TemplateWindowSize"], out var templateWindowSize) ? templateWindowSize : FastNlMeansColoredDenoisingSettings.Default.TemplateWindowSize,
                int.TryParse(section?["SearchWindowSize"], out var searchWindowSize) ? searchWindowSize : FastNlMeansColoredDenoisingSettings.Default.SearchWindowSize
            );
        }

        public static MedianBlurDenoisingSettings GetMedianBlurDenoisingSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Denoising:MedianBlur");

            return new MedianBlurDenoisingSettings(int.TryParse(section?["K"], out var k) ? k : MedianBlurDenoisingSettings.Default.K);
        }

        public static DenoisingSettings GetDenoisingSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Denoising");

            return new DenoisingSettings(section?["Algorithm"] ?? DenoisingSettings.Default.Algorithm);
        }

        public static SubtractionSettings GetSubtractionSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            IConfigurationSection section = config.GetSection("CV:Subtraction");

            return new SubtractionSettings(section?["Algorithm"] ?? SubtractionSettings.Default.Algorithm);
        }

        public static CNTSubtractorSettings GetCNTSubtractorSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Subtraction:CNT");

            return new CNTSubtractorSettings(
                int.TryParse(section?["MinPixelStability"], out var minPixelStability) ? minPixelStability : CNTSubtractorSettings.Default.MinPixelStability,
                bool.TryParse(section?["UseHistory"], out var useHistory) ? useHistory : CNTSubtractorSettings.Default.UseHistory,
                int.TryParse(section?["MaxPixelStability"], out var maxPixelStability) ? maxPixelStability : CNTSubtractorSettings.Default.MaxPixelStability,
                bool.TryParse(section?["IsParallel"], out var isParallel) ? isParallel : CNTSubtractorSettings.Default.IsParallel
            );
        }

        public static CorrectionSettings GetCorrectionSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Correction");

            return new CorrectionSettings(section?["Algorithm"] ?? CorrectionSettings.Default.Algorithm);
        }

        public static StaticMaskSettings GetStaticMaskSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Correction:StaticMask");
            return new StaticMaskSettings(section?["PathToFile"] ?? StaticMaskSettings.Default.MaskPath);
        }

        public static DetectionSettings GetDetectionSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            IConfigurationSection section = config.GetSection("CV:Detection");

            return new DetectionSettings(
                double.TryParse(section?["Threshold"], out var result)
                    ? result
                    : DetectionSettings.Default.DetectionThreshold);
        }
    }
}