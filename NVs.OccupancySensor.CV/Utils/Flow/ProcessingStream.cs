using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class ProcessingStream : Stream
    {
        private readonly Counter counter;
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly bool requiresSynchronizationOnReset;


        protected ProcessingStream(Counter counter, CancellationToken ct, ILogger logger, bool requiresSynchronizationOnReset = false) : base(ct, logger)
        {
            this.counter = counter;
            this.requiresSynchronizationOnReset = requiresSynchronizationOnReset;
        }

        public bool Completed { get; private set; }

        public void Process(Image<Gray, byte> image)
        {
            Logger.LogDebug("Received new frame...");
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
                Logger.LogDebug("Operation applied!");
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

        public void Reset()
        {
            Logger.LogDebug("Reset requested.");
            if (requiresSynchronizationOnReset)
            {
                lock (processingLock)
                {
                    DoReset();
                }
            }
            else
            {
                DoReset();
            }
        }

        protected virtual void DoReset() { }

        protected abstract Image<Gray, byte> DoProcess(Image<Gray, byte> image);
    }
}