using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Impl
{
    sealed class Camera : ICamera
    {
        private readonly VideoCapture videoCapture;
        private readonly List<IObserver<Mat>> observers = new List<IObserver<Mat>>();
        private readonly object observersLock = new object();
        private readonly CancellationTokenSource cts;
        private readonly ILogger<Camera> logger;
        private readonly TimeSpan frameInterval;

        private int framesCaptured = 0;
        public Camera(VideoCapture videoCapture, CancellationTokenSource cts, ILogger<Camera> logger, TimeSpan frameInterval)
        {
            this.videoCapture = videoCapture ?? throw new ArgumentNullException(nameof(videoCapture));
            this.cts = cts ?? throw new ArgumentNullException(nameof(cts));
            this.logger = logger;
            this.frameInterval = frameInterval;

            Task.Run(QueryFrames, cts.Token);
        }


        public IDisposable Subscribe(IObserver<Mat> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            // ReSharper disable InconsistentlySynchronizedField
            if (!observers.Contains(observer))
            {
                lock (observersLock)
                {
                    if (!observers.Contains(observer))
                    {
                        observers.Add(observer);
                    }
                }
            }

            return new Unsubscriber(observers, observersLock, observer);
            // ReSharper restore InconsistentlySynchronizedField
        }

        private async Task QueryFrames()
        {
            while (!cts.IsCancellationRequested)
            {
                logger.LogInformation($"Capturing frame {framesCaptured + 1}");
                Mat frame = null;
                try
                {
                    frame = videoCapture.QueryFrame();
                    logger.LogInformation("Got new frame");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to query frame!");
                    Notify(o => o.OnError(e));
                    Notify(o => o.OnCompleted());

                    return;
                }

                if (frame != null)
                {
                    Notify(o => o.OnNext(frame));
                }

                
                ++framesCaptured;
                logger.LogInformation($"Frame {framesCaptured} processed");
                
                if (framesCaptured == int.MaxValue - 1)
                {
                    logger.LogInformation("Resetting captured frames counter since it reached int.MaxValue - 1");
                    framesCaptured = 0;
                }
                
                
                if (!cts.IsCancellationRequested)
                {
                    await Task.Delay(frameInterval);
                }
            }

            logger.LogInformation("Cancellation requested");
            Notify(o => o.OnCompleted(), true);
        }

        private void Notify(Action<IObserver<Mat>> action, bool ignoreCancellation = false)
        {
            if (!ignoreCancellation && cts.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested before observers were notified");
                return;
            }

            IObserver<Mat>[] targets;
            lock (observersLock)
            {
                targets = new IObserver<Mat>[observers.Count];
                observers.CopyTo(targets);
            }

            foreach (var observer in targets)
            {
                Task.Run(() =>
                {
                    if (!ignoreCancellation && cts.IsCancellationRequested)
                    {
                        logger.LogInformation($"[Observer {observer.GetHashString()}] Cancellation requested before observer was notified");
                        return;
                    }

                    try
                    {
                        action(observer);
                        logger.LogInformation($"[Observer {observer.GetHashString()}] Notification succeeded");
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"[Observer {observer.GetHashString()}] Unable to notify observer!");
                    }
                });
            }
            logger.LogInformation("Notifications submitted");
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<Mat>> observers;
            private readonly object observersLock;
            private readonly IObserver<Mat> target;

            public Unsubscriber(List<IObserver<Mat>> observers, object observersLock, IObserver<Mat> target)
            {
                this.observers = observers ?? throw new ArgumentNullException(nameof(observers));
                this.target = target ?? throw new ArgumentNullException(nameof(target));
                this.observersLock = observersLock;
            }

            public void Dispose()
            {
                if (!observers.Contains(target))
                {
                    return;
                }

                lock (observersLock)
                {
                    if (!observers.Contains(target))
                    {
                        return;
                    }

                    observers.Remove(target);
                }
            }
        }
    }
}