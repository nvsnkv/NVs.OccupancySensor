using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class ProcessingStream : Stream
    {
        private readonly Counter counter;
        private readonly ProcessingLock processingLock = new ProcessingLock();


        protected ProcessingStream(Counter counter, CancellationToken ct, ILogger logger) : base(ct, logger)
        {
            this.counter = counter ?? throw new ArgumentNullException(nameof(counter));
        }

        public bool Completed { get; private set; }

        public void Process(Image<Gray, byte> image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Logger.LogInformation("Received new frame...");
            if (!processingLock.Acquire())
            {
                counter.IncreaseDropped();
                Logger.LogWarning("Previous operation is still in progress, frame will be dropped!");
                return;
            }

            if (Ct.IsCancellationRequested)
            {
                Logger.LogWarning("Cancellation requested, operation would not be applied!");
                return;
            }

            if (Completed)
            {
                Logger.LogWarning("Stream is completed, operation would not be applied!");
                return;
            }

            Image<Gray, byte> processed;
            try
            {
                processed = DoProcess(image);
                counter.IncreaseProcessed();
                Logger.LogInformation("Operation applied!");
            }
            catch (Exception e)
            {
                counter.IncreaseErrors();
                Logger.LogError(e, "Failed to apply operation!");
                Notify(o => o.OnError(e));
                Notify(o => o.OnCompleted());

                return;
            }
            finally
            {
                processingLock.Release();
            }

            Notify(o => o.OnNext(processed));
        }

        public void Complete()
        {
            Notify(o => o.OnCompleted());
            Completed = true;
        }

        public void Error(Exception error)
        {
            Notify(o => o.OnError(error));
        }

        protected abstract Image<Gray, byte> DoProcess(Image<Gray, byte> image);
    }
}