using System;
using System.Threading;
using Emgu.CV;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NVs.OccupancySensor.API.CV
{
    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCamera(this IServiceCollection services)
        {
            return services.AddSingleton<IObservable<Mat>>(s =>
            {
                var logger = s.GetService<ILogger<Camera>>();
                var config = s.GetService<IConfiguration>();
                var cvSource = config.GetSection("CV")["Source"];

                var capture = int.TryParse(cvSource, out int cameraIndex)
                    ? new VideoCapture(cameraIndex)
                    : new VideoCapture(cvSource);

                var cvFrameInterval = config.GetSection("CV")["FrameInterval"];
                if (!TimeSpan.TryParse(cvFrameInterval, out var frameInterval))
                {
                    frameInterval = TimeSpan.FromMilliseconds(100);
                }

                return new Camera(capture, new CancellationTokenSource(), logger, frameInterval);
            });
        }

        public static IServiceCollection AddRawImageObservers(this IServiceCollection services)
        {
            return services.AddScoped<RawImageObserver>(s =>
                new RawImageObserver(s.GetService<ILogger<RawImageObserver>>()));
        }
    }
}