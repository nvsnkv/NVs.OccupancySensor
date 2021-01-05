using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using NVs.OccupancySensor.CV.Transformation;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    sealed class TestTransform : ITransform
    {
        private readonly Func<TestTransform, TestTransform> copyFunc;

        public TestTransform(Func<TestTransform, TestTransform> copyFunc = null)
        {
            this.copyFunc = copyFunc ?? (t => t);
        }
        
        public DateTime? ApplyInvokedOn {get; private set; }
        public bool Disposed { get; private set; }
        
        public void Dispose()
        {
            Disposed = true;
        }

        public object Apply(object input)
        {
            ApplyInvokedOn = DateTime.Now;
            Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
            if (!(input is Image<Gray, byte>)) {
                return (input as Image<Rgb, byte>).Convert<Gray, byte>();
            }
            return input;
        }

        public ITransform Copy()
        {
            return copyFunc(this);
        }
    }
}