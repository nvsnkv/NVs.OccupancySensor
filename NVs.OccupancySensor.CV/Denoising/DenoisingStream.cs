using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Denoising.Denoisers;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.Denoising
{
    internal sealed class DenoisingStream : Stream<Image<Rgb, byte>>
    {
        private readonly ProcessingLock processingLock = new ProcessingLock(); 
        private readonly IDenoisingStrategy strategy;

        public DenoisingStream([NotNull] IDenoisingStrategy strategy, CancellationToken ct, [NotNull] ILogger logger) : base(ct, logger)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public void Process(Image<Rgb, byte> image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            logger.LogInformation("Received new frame to denoise...");
            if (!processingLock.Acquire())
            {
                logger.LogWarning("Previous operation is still in progress, frame will be dropped!");
                return;
            }

            Image<Rgb, byte> denoised;
            try
            {
                denoised = strategy.Denoise(image);
                logger.LogInformation("Noise reduction strategy applied!");
            }
            catch(Exception e)
            {
                logger.LogError(e, "Failed to apply denoising strategy!");
                Notify(o => o.OnError(e));
                Notify(o => o.OnCompleted());

                return;
            }

            Notify(o => o.OnNext(denoised));
        }

        public void Complete()
        {
            Notify(o => o.OnCompleted());
        }
    }
}