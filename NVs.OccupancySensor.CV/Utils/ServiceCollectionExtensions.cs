using System;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.DecisionMaking;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Sense;

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
                    s.GetService<IConfiguration>()?.GetCaptureSettings() ?? throw new InvalidOperationException("CaptureSettings were not resolved"),
                    Camera.CreateVideoCapture));

            services.AddSingleton<IDenoiserFactory>(s => new DenoiserFactory(
                s.GetService<IConfiguration>()?.GetFastNlMeansColoredDenoisingSettings() ?? throw new InvalidOperationException("FastNlMeansColoredDenoising settings dependency was not resolved"),
                s.GetService<IConfiguration>()?.GetMedianBlurDenoisingSettings() ?? throw new InvalidOperationException("MedianBlurDenoising settings dependency was not resolved")));

            services.AddSingleton<IDenoiser>(s => new Denoiser(
                s.GetService<IDenoiserFactory>() ?? throw new InvalidOperationException("DenoiserFactory dependency was not resolved"), 
                s.GetService<IConfiguration>()?.GetDenoisingSettings() ?? throw new InvalidOperationException("Denoising settings dependency was not resolved"),
                s.GetService<ILogger<Denoiser>>() ?? throw new InvalidOperationException("Denoiser logger dependency was not resolved")));
            
            services.AddSingleton<IDecisionMaker>(s => new DecisionMaker(s.GetService<ILogger<DecisionMaker>>() ?? throw new InvalidOperationException("DecisionMaker logger dependency was not resolved"))
            {
                Settings = s.GetService<IConfiguration>()?.GetDetectionSettings() ?? throw new InvalidOperationException("DecisionMaker configuration was not resolved")
            });

            services.AddSingleton<IBackgroundSubtractorFactory>(s => new BackgroundSubtractorFactory(s.GetService<IConfiguration>()?.GetCNTSubtractorSettings() ?? throw new InvalidOperationException("CNTSubtractor settings dependency was not resolved")));
            
            services.AddSingleton<IBackgroundSubtractionBasedDetector>(s => new BackgroundSubtractionBasedDetector(
                s.GetService<IBackgroundSubtractorFactory>() ?? throw new InvalidOperationException("BackgroundSubtractorFactory dependency was not resolved"),
                s.GetService<IDecisionMaker>() ?? throw new InvalidOperationException("DecisionMaker dependency was not resolved!"),
                s.GetService<ILogger<BackgroundSubtractionBasedDetector>>() ?? throw new InvalidOperationException("BackgroundSubtractionBasedDetector logger dependency was not resolved!"),
                s.GetService<IConfiguration>()?.GetDetectionSettings() ?? throw new InvalidOperationException("Detection settings were not resolved")
                ));

            services.AddSingleton<IPeopleDetector>(s => s.GetService<IBackgroundSubtractionBasedDetector>());

            services.AddSingleton<IOccupancySensor>(
                s => new Sense.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IDenoiser>() ?? throw new InvalidOperationException("Denoiser dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<ILogger<Sense.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver<Rgb>>(s => new RawImageObserver<Rgb>(s.GetService<ILogger<RawImageObserver<Rgb>>>() ?? throw new InvalidOperationException("RawImageObserver<Rgb> logger dependency was not resolved")));
            services.AddScoped<IImageObserver<Gray>>(s => new RawImageObserver<Gray>(s.GetService<ILogger<RawImageObserver<Gray>>>() ?? throw new InvalidOperationException("RawImageObserver<Gray> logger dependency was not resolved")));

            return services;
        }
    }
}