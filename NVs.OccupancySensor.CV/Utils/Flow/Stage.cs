using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils.Flow
{
    internal abstract class Stage : IObserver<Image<Gray, byte>>
    {
        private readonly ProcessingLock processingLock = new ProcessingLock();
        private readonly object streamLock = new object();
        protected readonly Counter Counter = new Counter();
        protected readonly ILogger Logger;

        protected volatile ProcessingStream? OutputStream;


        public IObservable<Image<Gray, byte>> Output
        {
            get
            {
                if (OutputStream != null) return OutputStream;
                lock (streamLock)
                {
                    // ReSharper disable once NonAtomicCompoundOperator - used within lock
                    OutputStream ??= CreateStream();
                }

                return OutputStream;
            }
        }

        protected Stage(ILogger logger)
        {
            Logger = logger;
        }

        public void OnCompleted()
        {
            Logger.LogInformation("Stream Completed. Setting Output to null.");
            OutputStream?.Complete();
        }

        public void OnError(Exception error)
        {
            Logger.LogWarning($"Error received! Setting output to null.{Environment.NewLine}, Exception:{error}");
            if (OutputStream == null)
            {
                lock (streamLock)
                {
                    // ReSharper disable once NonAtomicCompoundOperator - used within lock
                    OutputStream ??= CreateStream();
                }
            }

            OutputStream.Error(error);
            OutputStream.Complete();
        }

        public void OnNext(Image<Gray, byte> value)
        {
            Logger.LogDebug("New frame received...");

            if (!processingLock.Acquire())
            {
                Logger.LogWarning("Previously started operation is still in progress, the frame will be dropped!");
                Counter.IncreaseDropped();
                return;
            }

            try
            {
                if (OutputStream == null)
                {
                    lock (streamLock)
                    {
                        // ReSharper disable once NonAtomicCompoundOperator - used within lock
                        OutputStream ??= CreateStream();
                    }
                }

                OutputStream.Process(value);
                Logger.LogDebug("Frame processed.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to apply noise filter!");
                throw;
            }
            finally
            {
                processingLock.Release();
            }
        }
        
        public IStatistics Statistics => Counter;

        protected abstract ProcessingStream CreateStream();
    }
}