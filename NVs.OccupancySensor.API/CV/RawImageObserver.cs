using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.API.CV
{
    sealed class RawImageObserver : IObserver<Mat>
    {
        private readonly ILogger logger;
        private readonly AutoResetEvent captureReceived = new AutoResetEvent(false);

        private volatile Mat capture;
        private volatile Exception exception;
        private volatile bool completed;

        public RawImageObserver(ILogger logger)
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

        public void OnNext(Mat value)
        {
            capture = value;
            SetFlag();
        }

        public Task<Image<Rgb, int>> GetImage()
        {
            var task = new Task<Image<Rgb,int>>(() =>
            {
                captureReceived.WaitOne();
                if (exception != null)
                {
                    var e = exception;
                    exception = null;
                    throw new IOException("Failed to receive frame!", e);
                }

                if (completed)
                {
                    completed = false;
                    return null;
                }

                if (capture == null)
                {
                    throw  new InvalidOperationException("Capture was not provided by observable, but neither OnError nor OnComplete were called. Unable to provide a JPEG data");
                }

                return capture.ToImage<Rgb, int>();
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