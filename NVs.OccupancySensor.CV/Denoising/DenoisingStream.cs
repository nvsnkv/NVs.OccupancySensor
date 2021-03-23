using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils.Flow;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class DenoisingStream : ProcessingStream<Image<Rgb, byte>, Image<Rgb, byte>>
    {
        private readonly IDenoisingStrategy strategy;

        public DenoisingStream([NotNull] IDenoisingStrategy strategy, CancellationToken ct, [NotNull] ILogger logger) : base(ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        protected override Image<Rgb, byte> DoProcess(Image<Rgb, byte> image)
        {
            return strategy.Denoise(image);
        }
    }
}