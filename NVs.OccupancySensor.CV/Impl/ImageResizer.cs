using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class ImageResizer : IImageResizer
    {
        private readonly ILogger<ImageResizer> logger;
        private IResizeSettings settings;

        public ImageResizer(IResizeSettings settings, ILogger<ImageResizer> logger)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            if (logger != null) this.logger = logger;
        }

        public Image<Rgb, float> Resize([NotNull] Image<Rgb, float> input)
        {
            logger.LogInformation("Attempting to resize image...");
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (input.Width <= Settings.TargetWidth && input.Height <= Settings.TargetHeight)
            {
                logger.LogWarning("Size of received image is less or equal to target size, image will be bypassed.");
                return input;
            }

            Image<Rgb, float> result;
            try
            {
                result = input.Resize(Settings.TargetWidth, Settings.TargetHeight, Inter.Linear);
                logger.LogInformation("Image successfully resized!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to resize image!");
                throw;
            }

            return result;
        }

        public IResizeSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }
    }
}