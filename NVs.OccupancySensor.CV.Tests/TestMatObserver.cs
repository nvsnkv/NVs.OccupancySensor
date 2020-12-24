using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emgu.CV;
using Moq;

namespace NVs.OccupancySensor.CV.Tests
{
    class TestMatObserver : IObserver<Mat>
    {
        public Dictionary<Mat, DateTime> ReceivedItems { get; } = new Dictionary<Mat, DateTime>();

        public bool StreamCompleted { get; private set; }

        public Exception Error { get; private set; }



        public void OnCompleted()
        {
            StreamCompleted = true;
        }

        public void OnError(Exception error)
        {
            Error = error;
        }

        public virtual void OnNext(Mat value)
        {
            ReceivedItems.Add(value, DateTime.Now);
        }
    }

    class HeavyTestMatObserver: TestMatObserver
    {
        public override void OnNext(Mat value)
        {
            base.OnNext(value);
            Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();
        }
    }
}