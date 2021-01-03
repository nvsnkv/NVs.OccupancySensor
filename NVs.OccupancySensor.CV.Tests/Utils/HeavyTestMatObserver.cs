using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    class HeavyTestMatObserver: TestImageObserver
    {
        private readonly TimeSpan fromMilliseconds;

        public HeavyTestMatObserver(TimeSpan fromMilliseconds)
        {
            this.fromMilliseconds = fromMilliseconds;
        }

        public override void OnNext(Image<Rgb, byte> value)
        {
            base.OnNext(value);
            Task.Delay(fromMilliseconds).Wait();
        }
    }
}