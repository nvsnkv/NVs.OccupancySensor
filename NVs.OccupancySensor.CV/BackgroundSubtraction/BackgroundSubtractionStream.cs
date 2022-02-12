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
    internal sealed class BackgroundSubtractionStream : ProcessingStream
    {
        private readonly ISubtractionStrategy strategy;

        public BackgroundSubtractionStream(ISubtractionStrategy strategy, Counter counter, CancellationToken ct, ILogger logger) : base(counter, ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        protected override Image<Gray, byte> DoProcess(Image<Gray, byte> image)
        {
            return strategy.GetForegroundMask(image);
        }
    }
}