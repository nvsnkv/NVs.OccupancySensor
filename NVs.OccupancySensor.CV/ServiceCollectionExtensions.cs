using System;
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
                    s.GetService<ILogger<Camera>>(),
                    s.GetService<ILogger<CameraStream>>(),
                    s.GetService<IConfiguration>().GetCvSettings(),
                    Camera.CreateVideoCapture));

            services.AddSingleton<IMatConverter>(s => new MatConverter(s.GetService<ILogger<MatConverter>>()));

            services.AddSingleton<IPeopleDetector>(s => new HogPeopleDetector(s.GetService<ILogger<HogPeopleDetector>>(), HOGDescriptorWrapper.Create));

            services.AddSingleton<IOccupancySensor>(
                s => new Impl.OccupancySensor(
                    s.GetService<ICamera>(),
                    s.GetService<IMatConverter>(),
                    s.GetService<IPeopleDetector>(),
                    s.GetService<ILogger<Impl.OccupancySensor>>()));

            services.AddScoped<IImageObserver>(s => new RawImageObserver(s.GetService<ILogger<RawImageObserver>>()));

            return services;
        }

        private static Settings GetCvSettings(this IConfiguration config)
        {
            var cvSource = config?.GetSection("CV")?["Source"] ?? Settings.Default.Source;
            var cvFrameInterval = config?.GetSection("CV")?["FrameInterval"];
            var cvTargetWidth = config?.GetSection("CV")?["TargetWidth"];
            var cvTargetHeight = config?.GetSection("CV")?["TargetWidth"];

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