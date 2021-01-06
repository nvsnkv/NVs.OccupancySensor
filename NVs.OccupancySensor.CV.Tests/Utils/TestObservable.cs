using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal sealed class TestObservable<T> : IObservable<T>
    {
        private readonly List<T> items;
        private readonly TimeSpan delay;
        private readonly TimeSpan completionDelay;

        public TestObservable(List<T> items, TimeSpan delay, TimeSpan completionDelay)
        {
            this.items = items;
            this.delay = delay;
            this.completionDelay = completionDelay;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Task.Run(async () => {
                foreach(var item in items) 
                {
                    await Task.Delay(delay);
                    var _ = Task.Run(() => observer.OnNext(item));
                }

                await Task.Delay(completionDelay);
                await Task.Run(() => observer.OnCompleted());
            });

            return new Mock<IDisposable>().Object;
        }
    }
}