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
            return services.AddSingleton<ICameraStream>(s =>
            {
                var logger = s.GetService<ILogger<CameraStream>>();
                var config = s.GetService<IConfiguration>();
                var cvSource = config?.GetSection("CV")?["Source"] ?? Settings.Default.Source;

                var capture = int.TryParse(cvSource, out int cameraIndex)
                    ? new VideoCapture(cameraIndex)
                    : new VideoCapture(cvSource);

                var cvFrameInterval = config?.GetSection("CV")?["FrameInterval"];
                if (!TimeSpan.TryParse(cvFrameInterval, out var frameInterval))
                {
                    frameInterval = Settings.Default.FrameInterval;
                }
               
                return new CameraStream(capture, cts.Token, logger, frameInterval);
            });
        }

        public static IServiceCollection AddRawImageObservers(this IServiceCollection services)
        {
            return services.AddScoped<IImageObserver>(s =>
                new RawImageObserver(s.GetService<ILogger<RawImageObserver>>()));
        }
    }
}