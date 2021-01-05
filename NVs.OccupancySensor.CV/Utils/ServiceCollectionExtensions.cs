using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Settings;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Background;

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
                    s.GetService<IConfiguration>()?.GetCameraSettings() ?? throw new InvalidOperationException("CaptureSettings were not resolved"),
                    Camera.CreateVideoCapture));

            services.AddSingleton(s => new BackgroundSubtraction(s.GetService<ILogger<BackgroundSubtraction>>() ?? throw new InvalidOperationException("BackgroundSubtraction logger dependency was not resolved!")));
            services.AddSingleton<IBackgroundSubtraction>(s => s.GetService<BackgroundSubtraction>());

            services.AddSingleton<IImageTransformer>(s =>
                new ImageTransformBuilder(s.GetService<ILogger<ImageTransformer>>)
                    .Append(((Image<Rgb, byte> i) => i.Resize(0.5, Inter.Linear)))
                    .Append((Image<Rgb, byte> i) => i.Convert<Gray, byte>())
                    .Append((Image<Gray, byte> i) =>
                    {
                        Image<Gray, byte> denoised = new Image<Gray, byte>(i.Width, i.Height);
                        CvInvoke.MedianBlur(i, denoised, 7);
                        return denoised;
                    })
                    .Append(s.GetService<BackgroundSubtraction>() ?? throw new InvalidOperationException("BackgroundSubtraction  dependency was not resolved"))
                    .Synchronized()
                    .Append((Image<Gray, byte> i) => i.Convert<Rgb, byte>())
                    .ToTransformer());

            services.AddSingleton<IPeopleDetector>(s => new ForegroundMaskBasedPeopleDetector(
                s.GetService<ILogger<ForegroundMaskBasedPeopleDetector>>() ?? throw new InvalidOperationException( "FgmaskBasedPeopleDetector logger dependency was not resolved!"),
                s.GetService<IConfiguration>()?.GetDetectorThreshold()?? throw new InvalidOperationException("DetectionSettings were not resolved")));
            
            services.AddSingleton<IOccupancySensor>(
                s => new Sense.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<IImageTransformer>() ?? throw new InvalidOperationException("ImageTransformerDependency was not resolved"),
                    s.GetService<ILogger<Sense.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver<Rgb>>(s => new RawImageObserver<Rgb>(s.GetService<ILogger<RawImageObserver<Rgb>>>() ?? throw new InvalidOperationException("RawImageObserver<Rgb> logger dependency was not resolved")));
            services.AddScoped<IImageObserver<Gray>>(s => new RawImageObserver<Gray>(s.GetService<ILogger<RawImageObserver<Gray>>>() ?? throw new InvalidOperationException("RawImageObserver<Gray> logger dependency was not resolved")));

            return services;
        }

        private static CaptureSettings GetCameraSettings([NotNull] this IConfiguration config)
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

        private static double GetDetectorThreshold([NotNull] this IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var threshold = config.GetSection("CV:Detection")["Threshold"] ?? string.Empty;

            return double.TryParse(threshold, out var result)
                ? result
                : DetectionSettings.Default.Threshold;
        }
    }
}