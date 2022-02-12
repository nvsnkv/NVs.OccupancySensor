using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class DenoisingStream : ProcessingStream
    {
        private readonly IDenoisingStrategy strategy;

        public DenoisingStream(IDenoisingStrategy strategy, Counter counter, CancellationToken ct, ILogger logger) : base(counter, ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        protected override Image<Gray, byte> DoProcess(Image<Gray, byte> image)
        {
            return strategy.Denoise(image);
        }
    }
}