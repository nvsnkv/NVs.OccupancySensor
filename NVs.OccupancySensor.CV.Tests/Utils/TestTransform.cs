using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Transformation;
using NVs.OccupancySensor.CV.Transformation.Grayscale;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal sealed class TestTransform : IGrayscaleTransform
    {        
        public DateTime? ApplyInvokedOn {get; private set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }

        public Image<Gray, byte> Apply(Image<Gray, byte> input)
        {
            ApplyInvokedOn = DateTime.Now;
            Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
            
            return input;
        }
    }
}