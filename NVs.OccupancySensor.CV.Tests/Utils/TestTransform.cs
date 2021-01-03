using System;
using System.Threading.Tasks;
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
            return input;
        }

        public ITransform Copy()
        {
            return copyFunc(this);
        }
    }
}