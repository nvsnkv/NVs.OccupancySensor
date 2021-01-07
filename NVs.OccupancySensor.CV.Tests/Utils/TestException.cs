using System;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    internal class TestException : Exception
    {
        public TestException() { }

        public TestException(string message) : base(message) { }
    }
}