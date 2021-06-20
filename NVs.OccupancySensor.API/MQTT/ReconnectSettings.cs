using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace NVs.OccupancySensor.API.MQTT
{
    internal sealed class ReconnectSettings
    {
        public ReconnectSettings([NotNull] IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var section =  configuration.GetSection("MQTT:Reconnect");
            if (section == null) return;

            AttemptsCount = int.TryParse(section["AttemptsCount"], out var c) ? c : 0;
            Interval = TimeSpan.TryParse(section["Interval"], out var t) ? t : TimeSpan.Zero;
        }

        public int AttemptsCount { get; }

        public TimeSpan Interval { get; }
    }
}