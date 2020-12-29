using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class MatConverter : IMatConverter
    {
        ILogger<MatConverter> logger;

        public MatConverter(ILogger<MatConverter> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Image<Rgb, int> Convert(Mat input) 
        {
            if (input == null) 
            {
                logger.LogError("null mat received!");
                throw new ArgumentNullException(nameof(input));
            }

            logger.LogInformation("Attempting to convert received input...");

            Image<Rgb, int> result;
            try 
            {
                result = input.ToImage<Rgb, int>();
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