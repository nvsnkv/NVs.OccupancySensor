using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Utils
{
    internal abstract class Stream<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();
        private readonly object observersLock = new object();
        protected readonly CancellationToken ct;
        protected readonly ILogger logger;

        protected Stream(CancellationToken ct, [NotNull] ILogger logger)
        {
            this.ct = ct;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ct.Register(() =>
            {
                logger.LogInformation("Cancellation requested");
                Notify(o => o.OnCompleted(), true);
            });

        }

        public IDisposable Subscribe(IObserver<T> observer)
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
        }

        protected void Notify(Action<IObserver<T>> action, bool ignoreCancellation = false)
        {
            if (!ignoreCancellation && ct.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested before observers were notified");
                return;
            }

            IObserver<T>[] targets;
            lock (observersLock)
            {
                targets = new IObserver<T>[observers.Count];
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
            private readonly List<IObserver<T>> observers;
            private readonly object observersLock;
            private readonly IObserver<T> target;

            public Unsubscriber(List<IObserver<T>> observers, object observersLock, IObserver<T> target)
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