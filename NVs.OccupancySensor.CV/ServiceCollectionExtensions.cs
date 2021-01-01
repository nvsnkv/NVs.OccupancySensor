using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Impl;
using NVs.OccupancySensor.CV.Impl.HOG;

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
                    s.GetService<IConfiguration>()?.GetCvSettings() ?? throw new InvalidOperationException("Settings were not resolved"),
                    Camera.CreateVideoCapture));

            services.AddSingleton<IMatConverter>(s => new MatConverter(s.GetService<ILogger<MatConverter>>() ?? throw new InvalidOperationException("MatConverter logger dependency was not resolved")));

            services.AddSingleton<IPeopleDetector>(s => new HogPeopleDetector(s.GetService<ILogger<HogPeopleDetector>>() ?? throw new InvalidOperationException("HogPeopleDetector logger dependency was not resolved"), HOGDescriptorWrapper.Create));

            services.AddSingleton<IImageResizer>(s => new ImageResizer(s.GetService<IConfiguration>()?.GetCvSettings() ?? throw new InvalidOperationException("Settings were not resolved"),
                s.GetService<ILogger<ImageResizer>>() ?? throw new InvalidOperationException("ImageResizer logger dependency was not resolved")));

            services.AddSingleton<IOccupancySensor>(
                s => new Impl.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IMatConverter>() ?? throw new InvalidOperationException("MatConverter dependency was not resolved"),
                    s.GetService<IImageResizer>() ?? throw new InvalidOperationException("ImageResizer dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<ILogger<Impl.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver>(s => new RawImageObserver(s.GetService<ILogger<RawImageObserver>>() ?? throw new InvalidOperationException("RawImageObserver logger dependency was not resolved")));

            return services;
        }

        private static Settings GetCvSettings([NotNull] this IConfiguration config)
        {
            var cvSource = config.GetSection("CV")?["Source"] ?? Settings.Default.Source;
            var cvFrameInterval = config.GetSection("CV")?["FrameInterval"];
            var cvTargetWidth = config.GetSection("CV")?["TargetWidth"];
            var cvTargetHeight = config.GetSection("CV")?["TargetWidth"];

            if (!TimeSpan.TryParse(cvFrameInterval, out TimeSpan frameInterval))
            {
                frameInterval = Settings.Default.FrameInterval;
            }

            if (!int.TryParse(cvTargetHeight, out int targetHeight))
            {
                targetHeight = Settings.Default.TargetHeight;
            }

            if (!int.TryParse(cvTargetWidth, out int targetWidth))
            {
                targetWidth = Settings.Default.TargetWidth;
            }

            return new Settings(cvSource, frameInterval, targetWidth, targetHeight);
        }
    }
}