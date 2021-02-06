using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Utils
{
    internal static class ConfigurationExtensions
    {
        internal static CaptureSettings GetCaptureSettings([NotNull] this IConfiguration config)
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

        internal static DetectionSettings GetDetectionSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var threshold = config.GetSection("CV:Detection")?["Threshold"] ?? string.Empty;

            return new DetectionSettings(
                double.TryParse(threshold, out var result) ? result : DetectionSettings.Default.DetectionThreshold,
                config.GetSection("CV:Detection")?["DataDir"] ?? DetectionSettings.Default.DataDir,
                config.GetSection("CV:Detection")?["Algorithm"] ?? DetectionSettings.Default.Algorithm);   
        }

        internal static TransformSettings GetTransformSettings([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var section = config.GetSection("CV:Transform");
            
            if (section == null)
            {
                return TransformSettings.Default;
            }
            
            if (!double.TryParse(section["ResizeFactor"], out var rF))
            {
                rF = TransformSettings.Default.ResizeFactor;
            }

            if (!int.TryParse(section["InputBlurKernelSize"], out var iK))
            {
                iK = TransformSettings.Default.InputBlurKernelSize;
            }
            
            if (!int.TryParse(section["OutputBlurKernelSize"], out var oK))
            {
                oK = TransformSettings.Default.OutputBlurKernelSize;
            }

            return new TransformSettings(rF, iK, oK);
        }
    }
}