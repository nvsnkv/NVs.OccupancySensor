using System;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

namespace NVs.OccupancySensor.CV.Transformation.Background
{
    internal sealed class BackgroundSubtraction: IGrayscaleTransform, IBackgroundSubtraction
    {
        private static readonly double MaxLearningRate = 1.0d;
        private static readonly double Delta = 0.02d;
        private readonly IAlgorithmModelStorage storage;
        private readonly ILogger<BackgroundSubtraction> logger;
        private readonly string name;
        private readonly BackgroundSubtractorCNT subtractor;
        private double learningRate;
        

        public BackgroundSubtraction([NotNull] IAlgorithmModelStorage storage, [NotNull] ILogger<BackgroundSubtraction> logger,
            [NotNull] string name = nameof(BackgroundSubtraction))
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            subtractor = new BackgroundSubtractorCNT();
            learningRate = MaxLearningRate;

            LoadAlgorithm();
        }

        private void LoadAlgorithm()
        {
            string content;
            try
            {
                content = storage.GetAlgorithm(name);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to retrieve algorithm from storage!");
                throw;
            }

            if (content == null)
            {
                logger.LogWarning($"No algorithm was previously saved with name {name}");
                return;
            }

            try
            {
                subtractor.LoadFromString(content);
                logger.LogInformation("Algorithm loaded");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to load retrieved algorithm!");
                throw;
            }
        }

        public void Dispose()
        {
            subtractor?.Dispose();
        }

        public Image<Gray, byte> Apply(Image<Gray, byte> input)
        {
            var mask = new Image<Gray, byte>(input.Width, input.Height);

            subtractor.Apply(input, mask, GetLearningRate());
            return mask;
        }

        private double GetLearningRate()
        {
            if (learningRate != 0)
            {
                if (learningRate <= double.Epsilon)
                {
                    learningRate = 0;
                    SaveAlgorithm();
                }
                else
                {
                    learningRate -= Delta;
                }
                logger.LogInformation($"learningRate updated to {learningRate}");
            }

            return Math.Min(learningRate, 1);
        }

        private void SaveAlgorithm()
        {
            var result = subtractor.SaveToString();
            try
            {
                storage.SaveAlgorithm(name, result);
                logger.LogInformation("Algorithm updated in the storage");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save algorithm!");
                throw;
            }
        }
        public void ResetModel()
        {
            learningRate = MaxLearningRate;
            logger.LogInformation($"learningRate reset to {learningRate}");
        }
    }
}