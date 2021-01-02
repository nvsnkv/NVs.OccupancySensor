using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Settings;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class ImageConverter : IImageConverter
    {
        private readonly ILogger<ImageConverter> logger;
        private ConversionSettings settings;

        public ImageConverter([NotNull] ConversionSettings settings, [NotNull] ILogger<ImageConverter> logger)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Image<Rgb, float> Convert([NotNull] Image<Rgb, float> input)
        {
            logger.LogInformation("Attempting to convert image...");
            if (input == null) throw new ArgumentNullException(nameof(input));

            Image<Rgb, float> result = input;
            
            result = Resize(result);

            result = ConvertColor(result);

            result = Rotate(result);
            
            logger.LogInformation("Image successfully converted!");
            return result;
        }

        private Image<Rgb, float> Rotate(Image<Rgb, float> input)
        {
            if (Settings.RotationAngle == 0)
            {
                logger.LogInformation("Image rotation was not requested, bypassing existing image");
                return input;
            }

            Image<Rgb, float> result;
            try
            {
                result = input.Rotate(Settings.RotationAngle, new Rgb(Color.Black), false);
                logger.LogInformation("Image successfully rotated!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to rotate image!");
                throw;
            }

            return result;
        }

        private Image<Rgb, float> Resize(Image<Rgb, float> input)
        {
            if (!settings.Resize)
            {
                logger.LogInformation("Resize is not requested, bypassing existing image");
                return input;
            }
            
            // ReSharper disable once PossibleInvalidOperationException - see Resize property definition;
            var targetSize = Settings.TargetSize.Value;
            if (input.Width <= targetSize.Width && input.Height <= targetSize.Height)
            {
                logger.LogWarning("Size of input image is less or equal to target image, bypassing existing image");
                return input;
            }

            Image<Rgb, float> result;
            
            try
            {
                result = input.Resize(targetSize.Width, targetSize.Height, Inter.Linear);
                logger.LogInformation("Image successfully resized!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to resize image!");
                throw;
            }

            return result;
        }

        private Image<Rgb, float> ConvertColor(Image<Rgb, float> input)
        {
            if (!Settings.GrayScale)
            {
                logger.LogInformation("Color conversion was not requested, bypassing existing image");
                return input;
            }

            Image<Rgb, float> result;
            try
            {
                result = input.Convert<Gray, float>().Convert<Rgb, float>();
                logger.LogInformation("Image successfully converted to grayscale!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to convert image to grayscale!");
                throw;
            }

            return result;
        }

        public ConversionSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }
    }
}