using System;
using System.Text;
using System.Threading;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;

namespace NVs.OccupancySensor.CV.Tests.Utils
{
    class TestLogFormatter : ILogFormatter
    {
        public string Format(
            int scopeLevel,
            string name,
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception exception)
        {
            var builder = new StringBuilder();

            builder.Append(DateTime.Now.ToString("[hh:mm:ss:ffffff] "));
            builder.Append($"[Thread: {Thread.CurrentThread.ManagedThreadId}] ");
            
            if (scopeLevel > 0)
            {
                builder.Append(' ', scopeLevel * 2);
            }

            builder.Append($"{logLevel} ");

            if (!string.IsNullOrEmpty(name))
            {
                builder.Append($"{name} ");
            }

            if (eventId.Id != 0)
            {
                builder.Append($"[{eventId.Id}]: ");
            }

            if (!string.IsNullOrEmpty(message))
            {
                builder.Append(message);
            }

            if (exception != null)
            {
                builder.Append($"\n{exception}");
            }

            return builder.ToString();
        }
    }
}