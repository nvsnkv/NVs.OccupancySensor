using System.Threading;

namespace NVs.OccupancySensor.CV.Utils
{
    internal sealed class Counter : IStatistics
    {
        private long processedFrames;

        private long droppedFrames;

        private long errors;

        public ulong ProcessedFrames => unchecked((ulong)processedFrames);

        public ulong DroppedFrames => unchecked((ulong)droppedFrames);

        public ulong Errors => unchecked((ulong)errors);

        public void IncreaseProcessed() => Interlocked.Increment(ref processedFrames);

        public void IncreaseDropped() => Interlocked.Increment(ref droppedFrames);

        public void IncreaseErrors() => Interlocked.Increment(ref errors);
    };
}