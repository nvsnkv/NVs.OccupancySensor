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
        public static IServiceCollection AddCamera(this IServiceCollection services, CancellationTokenSource cts)
        {
            return services.AddSingleton<ICamera>(s =>
            {
                var logger = s.GetService<ILogger<Camera>>();
                var config = s.GetService<IConfiguration>();
                var cvSource = config?.GetSection("CV")?["Source"] ?? DefaultSettings.Source;

                var capture = int.TryParse(cvSource, out int cameraIndex)
                    ? new VideoCapture(cameraIndex)
                    : new VideoCapture(cvSource);

                var cvFrameInterval = config?.GetSection("CV")?["FrameInterval"];
                if (!TimeSpan.TryParse(cvFrameInterval, out var frameInterval))
                {
                    frameInterval = DefaultSettings.FrameInterval;
                }
               
                return new Camera(capture, cts, logger, frameInterval);
            });
        }

        public static IServiceCollection AddRawImageObservers(this IServiceCollection services)
        {
            return services.AddScoped<IImageObserver>(s =>
                new RawImageObserver(s.GetService<ILogger<RawImageObserver>>()));
        }
    }
}