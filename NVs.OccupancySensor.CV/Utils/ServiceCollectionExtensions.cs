using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Detection.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Background;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

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

            services.AddSingleton<IAlgorithmModelStorage>(s => new FileBasedAlgorithmStorage(s.GetService<IConfiguration>()?.GetDetectionSettings()?.DataDir ?? throw new InvalidOperationException("DetectionSettings were not resolved")));

            services.AddSingleton(s => new BackgroundSubtraction(
                       s.GetService<IAlgorithmModelStorage>() ?? throw new InvalidOperationException(),
                s.GetService<ILogger<BackgroundSubtraction>>() ?? throw new InvalidOperationException("BackgroundSubtraction logger dependency was not resolved!")));
            
            services.AddSingleton<IBackgroundSubtraction>(s => s.GetService<BackgroundSubtraction>());

            services.AddSingleton(s =>
            {
                var transformSettings = s.GetService<IConfiguration>()?.GetTransformSettings() ?? throw new InvalidOperationException("TransformSettings were not resolved!");
                return new GrayscaleStreamTransformerBuilder(s.GetService<ILogger<GrayscaleStreamTransformer>>)
                    .Append(Transforms.Resize(transformSettings.ResizeFactor))
                    .Append(Transforms.MedianBlur(transformSettings.InputBlurKernelSize))
                     .Append(s.GetService<BackgroundSubtraction>() ?? throw new InvalidOperationException("BackgroundSubtraction  dependency was not resolved"))
                    .Synchronized()
                    .Append(Transforms.MedianBlur(transformSettings.OutputBlurKernelSize))
                    .ToTransformer();
            });

            services.AddSingleton<IDecisionMaker>(s => new DecisionMaker(s.GetService<ILogger<DecisionMaker>>()){ Settings = s.GetService<IConfiguration>()?.GetDetectionSettings()});

            services.AddSingleton<IPeopleDetector>(s => new CNTBackgroundSubtractionBasedPeopleDetector(
                s.GetService<IDecisionMaker>() ?? throw new InvalidOperationException("DecisionMaker dependency was not resolved!"),
                s.GetService<ILogger<CNTBackgroundSubtractionBasedPeopleDetector>>() ?? throw new InvalidOperationException("FgmaskBasedPeopleDetector logger dependency was not resolved!")
                ));

            services.AddSingleton<IOccupancySensor>(
                s => new Sense.OccupancySensor(
                    s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera dependency was not resolved"),
                    s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector dependency was not resolved"),
                    s.GetService<IGrayscaleStreamTransformer>() ?? throw new InvalidOperationException("ImageTransformerDependency was not resolved"),
                    s.GetService<ILogger<Sense.OccupancySensor>>() ?? throw new InvalidOperationException("OccupancySensor logger dependency was not resolved")));

            services.AddScoped<IImageObserver<Rgb>>(s => new RawImageObserver<Rgb>(s.GetService<ILogger<RawImageObserver<Rgb>>>() ?? throw new InvalidOperationException("RawImageObserver<Rgb> logger dependency was not resolved")));
            services.AddScoped<IImageObserver<Gray>>(s => new RawImageObserver<Gray>(s.GetService<ILogger<RawImageObserver<Gray>>>() ?? throw new InvalidOperationException("RawImageObserver<Gray> logger dependency was not resolved")));

            return services;
        }
    }
}