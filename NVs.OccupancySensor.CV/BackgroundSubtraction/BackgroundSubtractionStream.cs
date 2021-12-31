using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.BackgroundSubtraction.Subtractors;
using NVs.OccupancySensor.CV.Utils;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.BackgroundSubtraction
{
    internal sealed class BackgroundSubtractionStream : ProcessingStream<Image<Gray, byte>, Image<Gray, byte>>
    {
        private readonly ISubtractionStrategy strategy;

        public BackgroundSubtractionStream([NotNull] ISubtractionStrategy strategy, [NotNull] Counter counter, CancellationToken ct, [NotNull] ILogger logger) : base(counter, ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        protected override Image<Gray, byte> DoProcess(Image<Gray, byte> image)
        {
            return strategy.GetForegroundMask(image);
        }
    }
}