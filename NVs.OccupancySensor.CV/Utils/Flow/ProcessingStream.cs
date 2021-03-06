using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class ProcessingStream<TIn, TOut> : Stream<TOut> where TIn: class
    {
        private readonly Counter counter;
        private readonly ProcessingLock processingLock = new ProcessingLock();


        protected ProcessingStream([NotNull] Counter counter, CancellationToken ct, [NotNull] ILogger logger) : base(ct, logger)
        {
            this.counter = counter ?? throw new ArgumentNullException(nameof(counter));
        }

        public bool Completed { get; private set; }

        public void Process(TIn image)
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

            TOut processed;
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

        protected abstract TOut DoProcess(TIn image);
    }
}