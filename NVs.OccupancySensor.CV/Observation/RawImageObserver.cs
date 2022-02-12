using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Observation
{
    internal sealed class RawImageObserver<TColor> : IImageObserver<TColor>
    where TColor: struct, IColor
    {
        private readonly ILogger<RawImageObserver<TColor>> logger;
        private readonly AutoResetEvent captureReceived = new AutoResetEvent(false);

        private volatile Image<TColor,byte>? capture;
        private volatile Exception? exception;
        private volatile bool completed;

        public RawImageObserver(ILogger<RawImageObserver<TColor>> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnCompleted()
        {
            completed = true;
            SetFlag();
        }

        public void OnError(Exception error)
        {
            exception = error;
            SetFlag();
        }

        public void OnNext(Image<TColor,byte>? value)
        {
            capture = value;
            SetFlag();
        }

        public Task<Image<TColor,byte>?> GetImage(CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                captureReceived.WaitOne();
                ct.ThrowIfCancellationRequested();

                if (exception != null)
                {
                    var e = exception;
                    exception = null;
                    logger.LogError(e, "Error received instead of frame!");
                    throw new IOException("Failed to receive frame!", e);
                }

                if (completed)
                {
                    completed = false;
                    return null;
                }

                if (capture == null)
                {
                    logger.LogError("null image received");
                }

                return capture;
            }, ct);
        }

        private void SetFlag()
        {
            if (!captureReceived.Set())
            {
                logger.LogWarning($"Failed to set {nameof(captureReceived)} flag, {nameof(GetImage)} method may get stuck");
            }
        }
    }
}