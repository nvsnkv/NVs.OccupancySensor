using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Correction
{
    sealed class CorrectionStream : ProcessingStream<Image<Gray, byte>, Image<Gray, byte>>
    {
        private readonly ICorrectionStrategy strategy;

        public CorrectionStream([NotNull] ICorrectionStrategy strategy, [NotNull] Counter counter, CancellationToken ct, [NotNull] ILogger logger) : base(counter, ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        protected override Image<Gray, byte> DoProcess(Image<Gray, byte> image)
        {
            return strategy.Apply(image);
        }
    }
}