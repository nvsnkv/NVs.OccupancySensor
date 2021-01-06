using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal class TestImageObserver : IObserver<Image<Rgb, byte>>
    {
        public Dictionary<Image<Rgb, byte>, DateTime> ReceivedItems { get; } = new Dictionary<Image<Rgb, byte>, DateTime>();

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

        public virtual void OnNext(Image<Rgb, byte> value)
        {
            ReceivedItems.Add(value, DateTime.Now);
        }
    }
}