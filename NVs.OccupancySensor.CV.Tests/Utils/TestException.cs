using System;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    class TestException : Exception
    {
        public TestException() { }

        public TestException(string message) : base(message) { }
    }
}