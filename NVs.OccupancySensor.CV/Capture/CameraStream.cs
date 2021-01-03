using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.CV.Capture
{
    sealed class CameraStream : ICameraStream
    {
        private readonly VideoCapture videoCapture;
        private readonly List<IObserver<Image<Rgb, byte>>> observers = new List<IObserver<Image<Rgb, byte>>>();
        private readonly object observersLock = new object();
        private readonly CancellationToken ct;
        private readonly ILogger<CameraStream> logger;
        private readonly TimeSpan frameInterval;

        private int framesCaptured;
        
        public CameraStream(VideoCapture videoCapture, CancellationToken ct, ILogger<CameraStream> logger, TimeSpan frameInterval)
        {
            this.videoCapture = videoCapture ?? throw new ArgumentNullException(nameof(videoCapture));
            this.ct = ct;
            this.logger = logger;
            this.frameInterval = frameInterval;
            
            this.ct.Register(() =>
            {
                logger.LogInformation("Cancellation requested");
                Notify(o => o.OnCompleted(), true);
            });

            Task.Run(QueryFrames, ct);
        }
        
        public IDisposable Subscribe(IObserver<Image<Rgb, byte>> observer)
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
            while (!ct.IsCancellationRequested)
            {
                logger.LogInformation($"Capturing frame {framesCaptured + 1}");
                Mat frame;
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
                    Image<Rgb, byte> image;
                    try
                    {
                        image = frame.ToImage<Rgb, byte>();
                        logger.LogInformation("Frame successfully converted to image!");
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to convert frame to image!");
                        throw;
                    }
                    Notify(o => o.OnNext(image));
                }
                else
                {
                    logger.LogWarning("null frame received");
                }

                
                ++framesCaptured;
                logger.LogInformation($"Frame {framesCaptured} processed");
                
                if (framesCaptured == int.MaxValue - 1)
                {
                    logger.LogInformation("Resetting captured frames counter since it reached int.MaxValue - 1");
                    framesCaptured = 0;
                }
                
                await Task.Delay(frameInterval, ct);
            }
        }

        private void Notify(Action<IObserver<Image<Rgb, byte>>> action, bool ignoreCancellation = false)
        {
            if (!ignoreCancellation && ct.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested before observers were notified");
                return;
            }

            IObserver<Image<Rgb, byte>>[] targets;
            lock (observersLock)
            {
                targets = new IObserver<Image<Rgb, byte>>[observers.Count];
                observers.CopyTo(targets);
            }

            foreach (var observer in targets)
            {
                // ReSharper disable once MethodSupportsCancellation - cancellation will be checked inside actions
                Task.Run(() =>
                {
                    if (!ignoreCancellation && ct.IsCancellationRequested)
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
            private readonly List<IObserver<Image<Rgb, byte>>> observers;
            private readonly object observersLock;
            private readonly IObserver<Image<Rgb, byte>> target;

            public Unsubscriber(List<IObserver<Image<Rgb, byte>>> observers, object observersLock, IObserver<Image<Rgb, byte>> target)
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