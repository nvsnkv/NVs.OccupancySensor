using System;
using System.Threading.Tasks;
using Emgu.CV;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    class HeavyTestMatObserver: TestMatObserver
    {
        private readonly TimeSpan fromMilliseconds;

        public HeavyTestMatObserver(TimeSpan fromMilliseconds)
        {
            this.fromMilliseconds = fromMilliseconds;
        }

        public override void OnNext(Mat value)
        {
            base.OnNext(value);
            Task.Delay(fromMilliseconds).Wait();
        }
    }
}