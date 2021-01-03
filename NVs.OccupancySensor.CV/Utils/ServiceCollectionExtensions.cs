using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Observervation;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Transformation;

namespace NVs.OccupancySensor.CV.Utils
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


            services.AddSingleton<IImageTransformer>(s =>
                new ImageTransformBuilder(s.GetService<ILogger<ImageTransformer>>)
                    .Append((Image<Rgb, byte> i) => i.Resize(0.5, Inter.Linear))
                    .Append((Image<Rgb, byte> i) => i.Convert<Gray, byte>())
                    .Append((Image<Gray, byte> i) =>
                    {
                        Image<Gray, byte> denoised = new Image<Gray, byte>(i.Width, i.Height);
                        CvInvoke.FastNlMeansDenoising(i, denoised);
                        return denoised;
                    })
                    .Append((Image<Gray, byte> i) => i.Convert<Rgb, byte>())
                    .ToTransformer());

            services.AddSingleton<IPeopleDetector>(new DummyPeopleDetector());
            
            services.AddSingleton<IOccupancySensor>(
                s => new Sense.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<IImageTransformer>() ?? throw new InvalidOperationException("ImageTransformerDependency was not resolved"),
                    s.GetService<ILogger<Sense.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver>(s => new RawImageObserver(s.GetService<ILogger<RawImageObserver>>() ?? throw new InvalidOperationException("RawImageObserver logger dependency was not resolved")));

            return services;
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