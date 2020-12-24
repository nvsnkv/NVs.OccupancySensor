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
                Mat frame = null;
                try
                {
                    frame = videoCapture.QueryFrame();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to query frame!");
                    Notify(o => o.OnError(e));
                }

                if (frame != null)
                {
                    Notify(o => o.OnNext(frame));
                }

                if (cts.IsCancellationRequested)
                {
                    Notify(o => o.OnCompleted(), true);
                }

                await Task.Delay(frameInterval);
            }
        }

        private void Notify(Action<IObserver<Mat>> action, bool ignoreCancellation = false)
        {
            if (!ignoreCancellation && cts.IsCancellationRequested)
            {
                return;
            }

            IObserver<Mat>[] targets;
            lock (observersLock)
            {
                targets = new IObserver<Mat>[observers.Count];
                observers.CopyTo(targets);
            }

            targets.AsParallel().ForAll(observer =>
            {
                if (!ignoreCancellation && cts.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    action(observer);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to notify observer!");
                }
            });
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