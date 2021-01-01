using System;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class MatConverter : IMatConverter
    {
        private readonly ILogger<MatConverter> logger;

        public MatConverter([NotNull] ILogger<MatConverter> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Image<Rgb, float> Convert(Mat input) 
        {
            if (input == null) 
            {
                logger.LogError("null mat received!");
                throw new ArgumentNullException(nameof(input));
            }

            logger.LogInformation("Attempting to convert received input...");

            Image<Rgb, float> result;
            try 
            {
                result = input.ToImage<Rgb, float>();
                logger.LogInformation("Successfully converted image");
            }
            catch (Exception e) 
            {
                logger.LogError(e, "Failed to convert received object to image!");
                throw;
            }

            return result;
        }
    }
}