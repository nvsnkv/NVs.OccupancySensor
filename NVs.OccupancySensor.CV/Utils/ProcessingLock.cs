// ReSharper disable InconsistentlySynchronizedField - double check locking
// TODO: find out why reshaprer raises this warning in this case
namespace NVs.OccupancySensor.CV.Utils
{
    internal sealed class ProcessingLock
    {
        private readonly object thisLock = new object();
        private volatile bool isProcessing;

        public bool Acquire()
        {
            if (isProcessing)
            {
                return false;
            }

            lock (thisLock)
            {
                if (isProcessing)
                {
                    return false;
                }

                isProcessing = true;
            }

            return true;
        }

        public void Release()
        {
            if (!isProcessing)
            {
                return;
            }

            lock (thisLock)
            {
                if (!isProcessing)
                {
                    return;
                }

                isProcessing = false;
            }
        }
    }
}