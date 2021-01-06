using System;
using System.Threading;
using JetBrains.Annotations;

namespace NVs.OccupancySensor.CV.Transformation
{
    internal sealed class SynchronizedTransform : ITypedTransform
    {
        private readonly ITypedTransform transform;
        private readonly object processingLock = new object();
        private readonly ManualResetEvent imageProcessed = new ManualResetEvent(false);

        private volatile bool isProcessing;
        private volatile object lastResult;

        public SynchronizedTransform([NotNull] ITypedTransform transform)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public void Dispose()
        {
            transform.Dispose();
        }

        public object Apply(object input)
        {
            if (!AcquireProcessingLock())
            {
                return GetLastResult();
            }

            object result;
            try
            {
                result = transform.Apply(input);
            }
            finally
            {
                ReleaseProcessingLock();
            }

            UpdateLastResult(result);

            return result;
        }

        private object GetLastResult()
        {
            if (lastResult == null)
            {
                imageProcessed.WaitOne();
            }

            return lastResult;
        }

        private void UpdateLastResult(object result)
        {
            var isFirstResult = lastResult == null;
            lastResult = result;
            if (isFirstResult)
            {
                imageProcessed.Set();
            }
        }

        private void ReleaseProcessingLock()
        {
            lock (processingLock)
            {
                isProcessing = false;
            }
        }

        private bool AcquireProcessingLock()
        {
            if (isProcessing)
            {
                return false;
            }

            lock (processingLock)
            {
                if (isProcessing)
                {
                    return false;
                }

                isProcessing = true;
            }

            return true;
        }

        public ITransform Copy()
        {
            return new SynchronizedTransform(transform);
        }

        public Type InType => transform.InType;

        public Type OutType => transform.OutType;
    }
}