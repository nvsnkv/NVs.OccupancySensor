using System;
using System.Collections.Generic;
using Emgu.CV;

namespace NVs.OccupancySensor.CV.Tests.Utils
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
}