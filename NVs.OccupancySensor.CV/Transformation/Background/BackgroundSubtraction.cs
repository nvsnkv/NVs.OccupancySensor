using System;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Transformation.Background
{
    sealed class BackgroundSubtraction: ITypedTransform, IBackgroundSubtraction
    {
        private static readonly double MaxLearningRate = 1.053d;
        private readonly ILogger<BackgroundSubtraction> logger;
        private readonly BackgroundSubtractorCNT subtractor;
        private double learningRate;
        

        public BackgroundSubtraction([NotNull] ILogger<BackgroundSubtraction> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            subtractor = new BackgroundSubtractorCNT();
            learningRate = MaxLearningRate;
        }
        
        public void Dispose()
        {
            subtractor?.Dispose();
        }

        public object Apply(object input)
        {
            var image = (Image<Gray, byte>) input;
            var mask = new Image<Gray, byte>(image.Width, image.Height);

            subtractor.Apply(image, mask, GetLearningRate());
            return mask;
        }

        private double GetLearningRate()
        {
            if (learningRate != 0)
            {
                if (learningRate <= double.Epsilon)
                {
                    learningRate = 0;
                }
                else
                {
                    learningRate = learningRate * 0.95;
                }
                logger.LogInformation($"learningRate updated to {learningRate}");
            }

            return Math.Min(learningRate, 1);
        }

        public ITransform Copy()
        {
            return new BackgroundSubtraction(logger);
        }

        public Type InType { get; } = typeof(Image<Gray, byte>);
        public Type OutType { get; } = typeof(Image<Gray, byte>);
        public void ResetModel()
        {
            learningRate = MaxLearningRate;
            logger.LogInformation($"learningRate reset to {learningRate}");
        }
    }
}