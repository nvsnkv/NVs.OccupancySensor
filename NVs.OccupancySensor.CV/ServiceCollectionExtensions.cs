using System;
using System.Threading;
using Emgu.CV;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Impl;

namespace NVs.OccupancySensor.CV
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCamera(this IServiceCollection services)
        {
            return services.AddSingleton<ICamera>(
                s => new Camera(
                    s.GetService<ILogger<Camera>>(),
                    s.GetService<ILogger<CameraStream>>(),
                    s.GetService<IConfiguration>().GetCvSettings(),
                    Camera.CreateVideoCapture));
        }

        public static IServiceCollection AddRawImageObservers(this IServiceCollection services)
        {
            return services.AddScoped<IImageObserver>(s =>
            {
                var observer = new RawImageObserver(s.GetService<ILogger<RawImageObserver>>());
                return observer;
            });
        }

        public static IServiceCollection AddMatConverter(this IServiceCollection services) 
        {
            services.AddSingleton<IMatConverter>(s => new MatConverter(s.GetService<ILogger<MatConverter>>()));
            return services;    
        }
    
        private static Settings GetCvSettings(this IConfiguration config)
        {
            var cvSource = config?.GetSection("CV")?["Source"] ?? Settings.Default.Source;
            var cvFrameInterval = config?.GetSection("CV")?["FrameInterval"];

            if (!TimeSpan.TryParse(cvFrameInterval, out TimeSpan frameInterval))
            {
                frameInterval = Settings.Default.FrameInterval;
            }

            return new Settings(cvSource, frameInterval);
        }
    }
}