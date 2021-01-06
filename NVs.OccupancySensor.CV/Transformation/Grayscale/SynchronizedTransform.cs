using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using JetBrains.Annotations;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

namespace NVs.OccupancySensor.CV.Transformation.Grayscale
{
    internal sealed class SynchronizedTransform : IGrayscaleTransform
    {
        private readonly IGrayscaleTransform transform;
        private readonly object processingLock = new object();
        private readonly ManualResetEvent imageProcessed = new ManualResetEvent(false);

        private volatile bool isProcessing;
        private volatile Image<Gray, byte> lastResult;

        public SynchronizedTransform([NotNull] IGrayscaleTransform transform)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public void Dispose()
        {
            transform.Dispose();
        }

        public Image<Gray, byte> Apply(Image<Gray, byte> input)
        {
            if (!AcquireProcessingLock())
            {
                return GetLastResult();
            }

            Image<Gray, byte> result;
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

        private Image<Gray, byte> GetLastResult()
        {
            if (lastResult == null)
            {
                imageProcessed.WaitOne();
            }

            return lastResult;
        }

        private void UpdateLastResult(Image<Gray, byte> result)
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
    }
}