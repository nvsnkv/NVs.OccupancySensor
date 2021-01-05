using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Transformation
{
    sealed class ImageTransformer : IImageTransformer
    {
        private readonly ILogger<ImageTransformer> logger;
        private readonly IReadOnlyList<ITransform> operations;

        internal ImageTransformer([NotNull] ILogger<ImageTransformer> logger, [NotNull] IReadOnlyList<ITransform>  operations)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.operations = operations ?? throw new ArgumentNullException(nameof(operations));
        }
        public Image<Gray, byte> Transform([NotNull] Image<Rgb, byte> input)
        {
            logger.LogInformation("Attempting to transform image...");
            if (input == null) throw new ArgumentNullException(nameof(input));

            object data = input;
            for (var i = 0; i < operations.Count; i++)
            {
                try
                {
                    data = operations[i].Apply(data);
                    logger.LogInformation($"[{i + 1}/{operations.Count}] transforms applied");
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed to apply transform [{i + 1}/{operations.Count}]!");
                    throw;
                }
            }

            switch (data)
            {
                case Image<Gray, byte> result:
                    return result;
                
                case null:
                    logger.LogError("Last transform returned null value!");
                    throw new InvalidOperationException("Last transform returned null value!");
                
                default:
                    logger.LogError(
                        $"Last transform returned something different from {typeof(Image<Gray, byte>)}. {data.GetType()} received");
                    throw new InvalidOperationException($"Last transform returned something different from {typeof(Image<Gray, byte>)}")
                    {
                        Data = {{"Actual type", data.GetType()}}
                    };
            }

        }

        public IImageTransformer GetPreviousTransformer()
        {
            return operations.Count <= 1 
                ? null 
                : new ImageTransformer(logger, operations.Take(operations.Count - 1).Select(o => o.Copy()).ToList().AsReadOnly());
        }

        public void Dispose()
        {
            foreach (var operation in operations)
            {
                operation.Dispose();
            }
        }
    }
}