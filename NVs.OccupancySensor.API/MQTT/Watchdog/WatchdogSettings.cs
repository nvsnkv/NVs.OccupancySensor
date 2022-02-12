using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace NVs.OccupancySensor.API.MQTT
{
    internal sealed class WatchdogSettings
    {
        public WatchdogSettings(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var section =  configuration.GetSection("MQTT:Reconnect");
            if (section == null) return;

            AttemptsCount = int.TryParse(section["AttemptsCount"], out var c) ? c : 0;
            Interval = TimeSpan.TryParse(section["IntervalBetweenAttempts"], out var t) ? t : TimeSpan.Zero;
        }

        public int AttemptsCount { get; }

        public TimeSpan Interval { get; } = TimeSpan.Zero;
    }
}