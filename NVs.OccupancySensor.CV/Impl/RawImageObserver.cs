using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class RawImageObserver : IImageObserver
    {
        private readonly ILogger<RawImageObserver> logger;
        private readonly AutoResetEvent captureReceived = new AutoResetEvent(false);

        private volatile Image<Rgb, float> capture;
        private volatile Exception exception;
        private volatile bool completed;

        public RawImageObserver(ILogger<RawImageObserver> logger)
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

        public void OnNext(Image<Rgb, float> value)
        {
            capture = value;
            SetFlag();
        }

        public Task<Image<Rgb, float>> GetImage()
        {
            var task = new Task<Image<Rgb, float>>(() =>
            {
                captureReceived.WaitOne();
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
                    throw new InvalidOperationException("Capture was not provided by observable, but neither OnError nor OnComplete were called.");
                    
                }

                return capture;
            });

            task.Start();
            return task;
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