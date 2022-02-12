using System;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Sense;

namespace NVs.OccupancySensor.CV.Utils
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPresenceDetection(this IServiceCollection services)
        {
            services.AddSingleton(s => new VideoCaptureProvider(s.GetService<IConfiguration>()?.GetCaptureSettings() ?? throw new InvalidOperationException("CaptureSettings were not resolved")));

            services.AddSingleton<ICamera>(
                s => new Camera(
                    s.GetService<ILogger<Camera>>() ?? throw new InvalidOperationException("Camera logger dependency was not resolved"),
                    s.GetService<ILogger<CameraStream>>() ?? throw new InvalidOperationException("CameraStream logger dependency was not resolved"),
                    s.GetService<IConfiguration>()?.GetCaptureSettings() ?? throw new InvalidOperationException("CaptureSettings were not resolved"),
                    (s.GetService<VideoCaptureProvider>() ?? throw new InvalidOperationException("VideoCaptureProvider dependency was not resolved")).Get));

            services.AddSingleton<IDenoiserFactory>(s => new DenoiserFactory(
                s.GetService<IConfiguration>()?.GetFastNlMeansColoredDenoisingSettings() ?? throw new InvalidOperationException("FastNlMeansColoredDenoising settings dependency was not resolved"),
                s.GetService<IConfiguration>()?.GetMedianBlurDenoisingSettings() ?? throw new InvalidOperationException("MedianBlurDenoising settings dependency was not resolved")));

            services.AddSingleton<IDenoiser>(s => new Denoiser(
                s.GetService<IDenoiserFactory>() ?? throw new InvalidOperationException("DenoiserFactory dependency was not resolved"),
                s.GetService<IConfiguration>()?.GetDenoisingSettings() ?? throw new InvalidOperationException("Denoising settings dependency was not resolved"),
                s.GetService<ILogger<Denoiser>>() ?? throw new InvalidOperationException("Denoiser logger dependency was not resolved")));


            services.AddSingleton<IBackgroundSubtractorFactory>(s => new BackgroundSubtractorFactory(s.GetService<IConfiguration>()?.GetCNTSubtractorSettings() ?? throw new InvalidOperationException("CNTSubtractor settings dependency was not resolved")));

            services.AddSingleton<IBackgroundSubtractor>(s => new BackgroundSubtractor(
                s.GetService<IBackgroundSubtractorFactory>() ?? throw new InvalidOperationException("BackgroundSubtractorFactory dependency was not resolved"),
                s.GetService<IConfiguration>()?.GetSubtractionSettings() ?? throw new InvalidOperationException("Detection settings were not resolved"),
            s.GetService<ILogger<BackgroundSubtractor>>() ?? throw new InvalidOperationException("BackgroundSubtractionBasedDetector logger dependency was not resolved!")
                ));

            services.AddSingleton<ICorrectionStrategyFactory>(s =>
                new CorrectionStrategyFactory(s.GetService<IConfiguration>()?.GetStaticMaskSettings() ?? throw new InvalidOperationException("ForegroundMaskCorrection settings dependency were not resolved")));

            services.AddSingleton<ICorrectionStrategyManager>(new CorrectionStrategyManager());

            services.AddSingleton<ICorrector>(s => new Corrector(
                s.GetService<ICorrectionStrategyFactory>() ?? throw new InvalidOperationException("CorrectionStrategyFactory dependency was not resolved!"),
                s.GetService<ICorrectionStrategyManager>() ?? throw new InvalidOperationException("CorrectionStrategy manager dependency was not resolved!"),
                s.GetService<IConfiguration>()?.GetCorrectionSettings() ?? throw new InvalidOperationException("Correction settings were not resolved!"),
                s.GetService<ILogger<Corrector>>() ?? throw new InvalidOperationException("Corrector logger dependency was not resolved")));

            services.AddSingleton<IPeopleDetector>(s => new PeopleDetector(s.GetService<IConfiguration>()?.GetDetectionSettings() ?? throw new InvalidOperationException("Detection settings dependency was not resolved!"),
                s.GetService<ILogger<PeopleDetector>>() ?? throw new InvalidOperationException("Detector logger dependency was not resolved!")));

            services.AddSingleton<IOccupancySensor>(
                s => new Sense.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IDenoiser>() ?? throw new InvalidOperationException("Denoiser dependency was not resolved"),
                    s.GetService<IBackgroundSubtractor>() ?? throw new InvalidOperationException("Subtractor dependency was not resolved"),
                    s.GetService<ICorrector>() ?? throw new InvalidOperationException("Corrector dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<ILogger<Sense.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver<Gray>>(s => new RawImageObserver<Gray>(s.GetService<ILogger<RawImageObserver<Gray>>>() ?? throw new InvalidOperationException("RawImageObserver<Gray> logger dependency was not resolved")));

            return services;
        }
    }
}