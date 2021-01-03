using System;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Impl.Detectors.HOG;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPresenceDetection(this IServiceCollection services)
        {
            services.AddSingleton<ICamera>(
                s => new Camera(
                    s.GetService<ILogger<Camera>>() ?? throw new InvalidOperationException("Camera logger dependency was not resolved"),
                    s.GetService<ILogger<CameraStream>>() ?? throw new InvalidOperationException("CameraStream logger dependency was not resolved"),
                    s.GetService<IConfiguration>()?.GetCameraSettings() ?? throw new InvalidOperationException("CameraSettings were not resolved"),
                    Camera.CreateVideoCapture));

            services.AddSingleton<IMatConverter>(s => new MatConverter(s.GetService<ILogger<MatConverter>>() ?? throw new InvalidOperationException("MatConverter logger dependency was not resolved")));

            services.AddSingleton<IPeopleDetector>(s => new HogPeopleDetector(s.GetService<ILogger<HogPeopleDetector>>() ?? throw new InvalidOperationException("HogPeopleDetector logger dependency was not resolved"), HOGDescriptorWrapper.Create));
            
            services.AddSingleton<IImageConverter>(s => new ImageConverter(s.GetService<IConfiguration>()?.GetConversionSettings() ?? throw new InvalidOperationException("Settings were not resolved"),
                s.GetService<ILogger<ImageConverter>>() ?? throw new InvalidOperationException("ImageConverter logger dependency was not resolved")));

            services.AddSingleton<IOccupancySensor>(
                s => new Impl.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IMatConverter>() ?? throw new InvalidOperationException("MatConverter dependency was not resolved"),
                    s.GetService<IImageConverter>() ?? throw new InvalidOperationException("ImageConverter dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<ILogger<Impl.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver>(s => new RawImageObserver(s.GetService<ILogger<RawImageObserver>>() ?? throw new InvalidOperationException("RawImageObserver logger dependency was not resolved")));

            return services;
        }

        private static ConversionSettings GetConversionSettings([NotNull] this IConfiguration config)
        {
            var conversionSection = config.GetSection("CV:Conversion");
            ConversionSettings conversionSettings;
            
            if (conversionSection == null)
            {
                conversionSettings = ConversionSettings.Default;
            }
            else
            {
                var tHeight = conversionSection["TargetHeight"];
                var tWidth = conversionSection["TargetWidth"];
                var tGrayScale = conversionSection["GrayScale"];
                var tRotationAngle = conversionSection["RotationAngle"];

                var size = (int.TryParse(tHeight, out int height) && int.TryParse(tWidth, out int width))
                    ? (Size?)new Size(width, height)
                    : null;

                if (!bool.TryParse(tGrayScale, out bool grayScale))
                {
                    grayScale = false;
                }

                if (!double.TryParse(tRotationAngle, out double rotationAngle))
                {
                    rotationAngle = 0;
                }

                conversionSettings = new ConversionSettings(size, grayScale, rotationAngle);
            }

            return conversionSettings;
        }

        private static CameraSettings GetCameraSettings(this IConfiguration config)
        {
            var cvSource = config.GetSection("CV:Camera")?["Source"] ?? CameraSettings.Default.Source;
            var cvFrameInterval = config.GetSection("CV:Camera")?["FrameInterval"] ?? string.Empty;

            if (!TimeSpan.TryParse(cvFrameInterval, out TimeSpan frameInterval))
            {
                frameInterval = CameraSettings.Default.FrameInterval;
            }

            var cameraSettings = new CameraSettings(cvSource, frameInterval);
            return cameraSettings;
        }
    }
}